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

using GoogleCloudSamples.Models;
using GoogleCloudSamples.Services;
using Google.Cloud.Vision.V1;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace GoogleCloudSamples.Controllers
{
    public class VisionAPIsController : Controller
    {
        /// <summary>
        /// How many VisionAPIs should we display on each page of the index?
        /// </summary>
        private const int _pageSize = 10;

        private readonly IVisionAPIStore _store;
        private readonly ImageUploader _imageUploader;

        public VisionAPIsController(IVisionAPIStore store, ImageUploader imageUploader)
        {
            _store = store;
            _imageUploader = imageUploader;
        }

        // GET: VisionAPIs
        public ActionResult Index(string nextPageToken)
        {
            return View(new ViewModels.VisionAPIs.Index()
            {
                VisionAPIList = _store.List(_pageSize, nextPageToken)
            });
        }

        // GET: VisionAPIs/Details/5
        public ActionResult Details(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            VisionAPI visionapi = _store.Read((long)id);
            if (visionapi == null)
            {
                return HttpNotFound();
            }

            return View(visionapi);
        }

        // GET: VisionAPIs/Create
        public ActionResult Create()
        {
            return ViewForm("Create", "Create");
        }

        // [START create]
        // POST: VisionAPIs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(VisionAPI visionapi, HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                _store.Create(visionapi);
                // If visionapi cover image submitted, save image to Cloud Storage
                if (image != null)
                {
                    var imageUrl = await _imageUploader.UploadImage(image, visionapi.Id);
                    visionapi.ImageUrl = imageUrl;
                    var client = ImageAnnotatorClient.Create();
                    // Load the image file into memory
                    var uploaded_image = Google.Cloud.Vision.V1.Image.FetchFromUri(imageUrl);
                    // Performs label detection on the image file
                    var response = client.DetectLabels(uploaded_image);
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    string json = js.Serialize(response);
                    visionapi.JsonDescription = json;
                    _store.Update(visionapi);
                }
                return RedirectToAction("Details", new { id = visionapi.Id });
            }
            return ViewForm("Create", "Create", visionapi);
        }
        // [END create]

        /// <summary>
        /// Dispays the common form used for the Edit and Create pages.
        /// </summary>
        /// <param name="action">The string to display to the user.</param>
        /// <param name="visionapi">The asp-action value.  Where will the form be submitted?</param>
        /// <returns>An IActionResult that displays the form.</returns>
        private ActionResult ViewForm(string action, string formAction, VisionAPI visionapi = null)
        {
            var form = new ViewModels.VisionAPIs.Form()
            {
                Action = action,
                VisionAPI = visionapi ?? new VisionAPI(),
                IsValid = ModelState.IsValid,
                FormAction = formAction
            };
            return View("/Views/VisionAPIs/Form.cshtml", form);
        }

        // GET: VisionAPIs/Edit/5
        public ActionResult Edit(long? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            VisionAPI visionapi = _store.Read((long)id);
            if (visionapi == null)
            {
                return HttpNotFound();
            }
            return ViewForm("Edit", "Edit", visionapi);
        }

        // POST: VisionAPIs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(VisionAPI visionapi, long id, HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                visionapi.Id = id;
                if (image != null)
                {
                    visionapi.ImageUrl = await _imageUploader.UploadImage(image, visionapi.Id);
                }
                _store.Update(visionapi);
                return RedirectToAction("Details", new { id = visionapi.Id });
            }
            return ViewForm("Edit", "Edit", visionapi);
        }

        // POST: VisionAPIs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(long id)
        {
            // Delete visionapi cover image from Cloud Storage if ImageUrl exists
            string imageUrlToDelete = _store.Read((long)id).ImageUrl;
            if (imageUrlToDelete != null)
            {
                await _imageUploader.DeleteUploadedImage(id);
            }
            _store.Delete(id);
            return RedirectToAction("Index");
        }
    }
}