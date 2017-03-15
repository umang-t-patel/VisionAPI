// Copyright(c) 2016 Google Inc.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not
// use this file except in compliance with the License. You may obtain a copy of
// the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
// License for the specific language governing permissions and limitations under
// the License.

using Google.Datastore.V1;
using Google.Protobuf;
using System;
using System.Linq;

namespace GoogleCloudSamples.Models
{
    public static class DatastoreVisionAPIStoreExtensionMethods
    {
        /// <summary>
        /// Make a datastore key given a VisionAPI's id.
        /// </summary>
        /// <param name="id">A VisionAPI's id.</param>
        /// <returns>A datastore key.</returns>
        public static Key ToKey(this long id) =>
            new Key().WithElement("VisionAPI", id);

        /// <summary>
        /// Make a VisionAPI id given a datastore key.
        /// </summary>
        /// <param name="key">A datastore key</param>
        /// <returns>A VisionAPI id.</returns>
        public static long ToId(this Key key) => key.Path.First().Id;

        /// <summary>
        /// Create a datastore entity with the same values as VisionAPI.
        /// </summary>
        /// <param name="VisionAPI">The VisionAPI to store in datastore.</param>
        /// <returns>A datastore entity.</returns>
        /// [START toentity]
        public static Entity ToEntity(this VisionAPI visionapi) => new Entity()
        {
            Key = visionapi.Id.ToKey(),
            ["Title"] = visionapi.Title,
            ["PublishedDate"] = visionapi.PublishedDate?.ToUniversalTime(),
            ["ImageUrl"] = visionapi.ImageUrl,
            ["JsonDescription"] = visionapi.JsonDescription,
            ["CreateById"] = visionapi.CreatedById
        };
        // [END toentity]

        /// <summary>
        /// Unpack a VisionAPI from a datastore entity.
        /// </summary>
        /// <param name="entity">An entity retrieved from datastore.</param>
        /// <returns>A VisionAPI.</returns>
        public static VisionAPI ToVisionAPI(this Entity entity) => new VisionAPI()
        {
            Id = entity.Key.Path.First().Id,
            Title = (string)entity["Title"],
            PublishedDate = (DateTime?)entity["PublishedDate"],
            ImageUrl = (string)entity["ImageUrl"],
            JsonDescription = (string)entity["JsonDescription"],
            CreatedById = (string)entity["CreatedById"]
        };
    }

    public class DatastoreVisionAPIStore : IVisionAPIStore
    {
        private readonly string _projectId;
        private readonly DatastoreDb _db;

        /// <summary>
        /// Create a new datastore-backed VisionAPIstore.
        /// </summary>
        /// <param name="projectId">Your Google Cloud project id</param>
        public DatastoreVisionAPIStore(string projectId)
        {
            _projectId = projectId;
            _db = DatastoreDb.Create(_projectId);
        }

        // [START create]
        public void Create(VisionAPI visionapi)
        {
            var entity = visionapi.ToEntity();
            entity.Key = _db.CreateKeyFactory("VisionAPI").CreateIncompleteKey();
            var keys = _db.Insert(new[] { entity });
            visionapi.Id = keys.First().Path.First().Id;
        }
        // [END create]

        public void Delete(long id)
        {
            _db.Delete(id.ToKey());
        }

        // [START list]
        public VisionAPIList List(int pageSize, string nextPageToken)
        {
            var query = new Query("VisionAPI") { Limit = pageSize };
            if (!string.IsNullOrWhiteSpace(nextPageToken))
                query.StartCursor = ByteString.FromBase64(nextPageToken);
            var results = _db.RunQuery(query);
            return new VisionAPIList()
            {
                VisionAPIs = results.Entities.Select(entity => entity.ToVisionAPI()),
                NextPageToken = results.Entities.Count == query.Limit ?
                    results.EndCursor.ToBase64() : null
            };
        }
        // [END list]

        public VisionAPI Read(long id)
        {
            return _db.Lookup(id.ToKey())?.ToVisionAPI();
        }

        public void Update(VisionAPI visionapi)
        {
            _db.Update(visionapi.ToEntity());
        }
    }
}