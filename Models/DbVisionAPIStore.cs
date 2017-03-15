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

using System.Linq;

namespace GoogleCloudSamples.Models
{
    /// <summary>
    /// Implements IVisionAPIStore with a database.
    /// </summary>
    public class DbVisionAPIStore : IVisionAPIStore
    {
        private readonly ApplicationDbContext _dbcontext;

        public DbVisionAPIStore(ApplicationDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        // [START create]
        public void Create(VisionAPI visionapi)
        {
            var trackVisionAPI = _dbcontext.VisionAPIs.Add(visionapi);
            _dbcontext.SaveChanges();
            visionapi.Id = trackVisionAPI.Id;
        }
        // [END create]
        public void Delete(long id)
        {
            VisionAPI visionapi = _dbcontext.VisionAPIs.Single(m => m.Id == id);
            _dbcontext.VisionAPIs.Remove(visionapi);
            _dbcontext.SaveChanges();
        }

        // [START list]
        public VisionAPIList List(int pageSize, string nextPageToken)
        {
            IQueryable<VisionAPI> query = _dbcontext.VisionAPIs.OrderBy(visionapi => visionapi.Id);
            if (nextPageToken != null)
            {
                long previousVisionAPIId = long.Parse(nextPageToken);
                query = query.Where(visionapi => visionapi.Id > previousVisionAPIId);
            }
            var visionapis = query.Take(pageSize).ToArray();
            return new VisionAPIList()
            {
                VisionAPIs = visionapis,
                NextPageToken = visionapis.Count() == pageSize ? visionapis.Last().Id.ToString() : null
            };
        }
        // [END list]

        public VisionAPI Read(long id)
        {
            return _dbcontext.VisionAPIs.Single(m => m.Id == id);
        }

        public void Update(VisionAPI visionapi)
        {
            _dbcontext.Entry(visionapi).State = System.Data.Entity.EntityState.Modified;
            _dbcontext.SaveChanges();
        }
    }
}