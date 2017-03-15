var CV_URL = 'https://vision.googleapis.com/v1/images:annotate?key=AIzaSyDKEEPM_JAD2cZPk2-STygTmqsGfKqi1J4';

function sendFileToCloudVision(ImageUri) {
  // Strip out the file prefix when you convert to json.
  var request = {
    requests: [{
      image: {
        source: {
            gcsImageUri: ImageUri
        }
      },	 
      //image: {
      //  content: content
      //},
      features: [{
        type: "FACE_DETECTION",
        maxResults: 200
      },{
        type: "LABEL_DETECTION",
        maxResults: 200
      },{
        type: "TEXT_DETECTION",
        maxResults: 200
      },{
        type: "LANDMARK_DETECTION",
        maxResults: 200
      },{
        type: "LOGO_DETECTION",
        maxResults: 200
      },
	  ]
    }]
  };
  $.post({
    url: CV_URL,
    data: JSON.stringify(request),
    contentType: 'application/json'
  }).fail(function (jqXHR, textStatus, errorThrown) {
    alert('ERRORS: ' + textStatus + ' ' + errorThrown);
  }).done(displayJSON);
}

/**
 * Displays the results.
 */
function displayJSON (data) {
  var contents = JSON.stringify(data, null, 4);
  $('#results').text(contents);
  var evt = new Event('results-displayed');
  evt.results = contents;
  document.dispatchEvent(evt);
}
