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

namespace GoogleCloudSamples.Models
{
    /// <summary>
    /// An interface for storing VisionAPIs.  Can be implemented by a database,
    /// Google Datastore, etc.
    /// </summary>
    public interface IVisionAPIStore
    {
        /// <summary>
        /// Creates a new VisionAPI.  The Id of the VisionAPI will be filled when the
        /// function returns.
        /// </summary>
        void Create(VisionAPI visionapi);

        VisionAPI Read(long id);

        void Update(VisionAPI visionapi);

        void Delete(long id);

        VisionAPIList List(int pageSize, string nextPageToken);
    }
}