﻿#region licence
// The MIT License (MIT)
// 
// Filename: Test05CreateServicesAsync.cs
// Date Created: 2014/07/19
// 
// Copyright (c) 2014 Jon Smith (www.selectiveanalytics.com & www.thereformedprogrammer.net)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
#endregion

using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using GenericServices.ServicesAsync.Concrete;
using NUnit.Framework;
using Tests.DataClasses;
using Tests.DataClasses.Concrete;
using Tests.DTOs.Concrete;
using Tests.Helpers;

namespace Tests.UnitTests.Group15CrudServiceFinder
{
    class Test05CreateServicesAsync
    {
        [TestFixtureSetUp]
        public void SetUpFixture()
        {
            using (var db = new SampleWebAppDb())
            {
                DataLayerInitialise.InitialiseThis();
                var filepath = TestFileHelpers.GetTestFileFilePath("DbContentSimple.xml");
                DataLayerInitialise.ResetDatabaseToTestData(db, filepath);
            }
        }

        [Test]
        public async Task Check03DetailDirectPostOk()
        {
            using (var db = new SampleWebAppDb())
            {
                //SETUP
                var service = new DetailServiceAsync(db);
                var firstPost = db.Posts.First();

                //ATTEMPT
                var status = await service.GetDetailAsync<Post>(firstPost.PostId);

                //VERIFY
                status.IsValid.ShouldEqual(true, status.Errors);
                status.Result.PostId.ShouldEqual(firstPost.PostId);
                status.Result.Title.ShouldEqual(firstPost.Title);
            }
        }

        [Test]
        public async Task Check04DetailDirectDtoOk()
        {
            using (var db = new SampleWebAppDb())
            {
                //SETUP
                var service = new DetailServiceAsync(db);
                var firstPost = db.Posts.Include(x => x.Tags).AsNoTracking().First();

                //ATTEMPT
                var status = await service.GetDetailAsync<SimplePostDtoAsync>(firstPost.PostId);

                //VERIFY
                status.IsValid.ShouldEqual(true, status.Errors);
                status.Result.PostId.ShouldEqual(firstPost.PostId);
                status.Result.BloggerName.ShouldEqual(firstPost.Blogger.Name);
                status.Result.Title.ShouldEqual(firstPost.Title);
                CollectionAssert.AreEqual(firstPost.Tags.Select(x => x.TagId), status.Result.Tags.Select(x => x.TagId));
            }
        }

        //-------------
        //update

        [Test]
        public async Task Check05UpdateSetupServicePost()
        {
            using (var db = new SampleWebAppDb())
            {
                //SETUP
                var firstPostUntracked = db.Posts.AsNoTracking().First();
                var service = new UpdateSetupServiceAsync(db);

                //ATTEMPT
                var status = await service.GetOriginalAsync<Post>(firstPostUntracked.PostId);

                //VERIFY
                status.IsValid.ShouldEqual(true, status.Errors);
                status.Result.ShouldNotEqualNull();
                status.Result.PostId.ShouldEqual(firstPostUntracked.PostId);
            }
        }

        [Test]
        public async Task Check05UpdateSetupServiceDto()
        {
            using (var db = new SampleWebAppDb())
            {
                //SETUP
                var firstPostUntracked = db.Posts.AsNoTracking().First();
                var service = new UpdateSetupServiceAsync(db);

                //ATTEMPT
                var status = await service.GetOriginalAsync<SimplePostDtoAsync>(firstPostUntracked.PostId);

                //VERIFY
                status.IsValid.ShouldEqual(true, status.Errors);
                status.Result.ShouldNotEqualNull();
                status.Result.PostId.ShouldEqual(firstPostUntracked.PostId);
            }
        }

        [Test]
        public async Task Check06UpdateDirectOk()
        {
            using (var db = new SampleWebAppDb())
            {
                //SETUP
                var snap = new DbSnapShot(db);
                var firstPostUntracked = db.Posts.AsNoTracking().First();
                var service = new UpdateServiceAsync(db);

                //ATTEMPT
                firstPostUntracked.Title = Guid.NewGuid().ToString();
                var status = await service.UpdateAsync(firstPostUntracked);

                //VERIFY
                status.IsValid.ShouldEqual(true, status.Errors);
                status.SuccessMessage.ShouldEqual("Successfully updated Post.");
                snap.CheckSnapShot(db);

            }
        }

        [Test]
        public async Task Check07UpdateDirectDtoOk()
        {
            using (var db = new SampleWebAppDb())
            {
                //SETUP
                var snap = new DbSnapShot(db);
                var firstPostUntrackedNoIncludes = db.Posts.AsNoTracking().First();
                var service = new UpdateServiceAsync(db);
                var setupStatus = await (new UpdateSetupServiceAsync(db)).GetOriginalAsync<SimplePostDtoAsync>(firstPostUntrackedNoIncludes.PostId);
                setupStatus.IsValid.ShouldEqual(true, setupStatus.Errors);

                //ATTEMPT
                setupStatus.Result.Title = Guid.NewGuid().ToString();
                var status = await service.UpdateAsync(setupStatus.Result);

                //VERIFY
                status.IsValid.ShouldEqual(true, status.Errors);
                snap.CheckSnapShot(db);
                var updatedPost = db.Posts.Include(x => x.Tags).First();
                updatedPost.Title.ShouldEqual(setupStatus.Result.Title);
                updatedPost.Content.ShouldEqual(firstPostUntrackedNoIncludes.Content);
            }
        }

        [Test]
        public async Task Check08UpdateResetDtoOk()
        {
            using (var db = new SampleWebAppDb())
            {
                //SETUP
                var service = new UpdateServiceAsync(db);
                var dto = new DetailPostDtoAsync();

                //ATTEMPT
                await service.ResetDtoAsync(dto);

                //VERIFY
                dto.Bloggers.ShouldNotEqualNull();
                dto.Bloggers.KeyValueList.Count.ShouldNotEqual(0);
            }
        }

        //--------------------------------------------
        //create

        [Test]
        public async Task Check10CreateSetupDtoOk()
        {
            using (var db = new SampleWebAppDb())
            {
                //SETUP
                var service = new CreateSetupServiceAsync(db);

                //ATTEMPT
                var result = await service.GetDtoAsync<SimplePostDtoAsync>();

                //VERIFY
                result.ShouldNotEqualNull();
            }
        }

        [Test]
        public async Task Check11CreateDirectOk()
        {
            using (var db = new SampleWebAppDb())
            {
                //SETUP
                var snap = new DbSnapShot(db);
                var service = new CreateServiceAsync(db);
                var firstPostUntracked = db.Posts.Include(x => x.Tags).AsNoTracking().First();
                var tagsTracked = db.Tags.ToList().Where(x => firstPostUntracked.Tags.Any(y => y.TagId == x.TagId)).ToList();

                //ATTEMPT
                firstPostUntracked.Title = Guid.NewGuid().ToString();
                firstPostUntracked.Tags = tagsTracked;
                var status = await service.CreateAsync(firstPostUntracked);

                //VERIFY
                status.IsValid.ShouldEqual(true);
                snap.CheckSnapShot(db, 1, 2);
                var updatedPost = db.Posts.OrderByDescending(x => x.PostId).Include(x => x.Tags).First();
                updatedPost.Title.ShouldEqual(firstPostUntracked.Title);
                updatedPost.BlogId.ShouldEqual(firstPostUntracked.BlogId);
                CollectionAssert.AreEqual(firstPostUntracked.Tags.Select(x => x.TagId), updatedPost.Tags.Select(x => x.TagId));
            }
        }

        [Test]
        public async Task Check12CreateResetDtoOk()
        {
            using (var db = new SampleWebAppDb())
            {
                //SETUP
                var service = new CreateServiceAsync(db);
                var dto = new DetailPostDtoAsync();

                //ATTEMPT
                await service.ResetDtoAsync(dto);

                //VERIFY
                dto.Bloggers.ShouldNotEqualNull();
                dto.Bloggers.KeyValueList.Count.ShouldNotEqual(0);
            }
        }
    }
}
