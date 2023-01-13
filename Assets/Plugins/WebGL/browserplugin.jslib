var BrowserPlugin = {

micStatus : 0,
buffer: undefined,


Init: function()
{
var recorder;
var gumStream;
var subject;
var filename;
this.a = 0;
console.log("init inside js");
},

WriteToFile:function(str,subj)
{
    lines = Pointer_stringify(str);
    subjectLine = Pointer_stringify(subj);
  var xhr=new XMLHttpRequest();
          xhr.onload=function(e) {
              if(this.readyState === 4) {
                  console.log("Server returned: ",e.target.responseText);
              }
          };
          console.log("writing line for  " + subjectLine);
          var fd=new FormData();
          fd.append("functionname","WriteLine");
          fd.append("subjectName",subjectLine);

          fd.append("line",lines);
          xhr.open("POST","functions.php",true);
          xhr.send(fd);

},


PutTextFile: function()
{

    console.log("about to send to s3");
    var xhr=new XMLHttpRequest();
          xhr.onload=function(e) {
              if(this.readyState === 4) {
                  console.log("Server returned: ",e.target.responseText);
              }
          };
          var fd=new FormData();
          fd.append("functionname","SendLogFile");
          fd.append("subjectName",subject);
          xhr.open("POST","functions.php",true);
          xhr.send(fd);
},

PrintString: function(str)
{
    console.log("str " + str);
    var string = Pointer_stringify(str);
    console.log(string);
    return string;
},

SubjectSet : function(subjName)
{
    // var date=new Date().toISOString();
     console.log("subj name is " + subjName)
    // console.log(date);
     //subject=date;
   // console.log("subject " + date);
    subject = Pointer_stringify(subjName);
    console.log(subject);
console.log("subject in js is " + subject);
  var xhr=new XMLHttpRequest();
          xhr.onload=function(e) {
              if(this.readyState === 4) {
                  console.log("Server returned: ",e.target.responseText);
              }
          };
          var fd=new FormData();

          fd.append("functionname","SetSubject");
          fd.append("arguments",subject);
          xhr.open("POST","functions.php",true);
          xhr.send(fd);

},


ReturnMicStatus:function()
{

console.log("returning mic status");
var temp =0;
return temp;
},

CheckMic: function()
{
//var unityInstance = UnityLoader.instantiate("unityContainer", "Build/SCIFI_WEB.json", {onProgress: UnityProgress});
if (navigator.mediaDevices.getUserMedia) {
 navigator.mediaDevices.getUserMedia({
            audio: true
        }).then(function(stream) {
        console.log("Accessed the microphone");
     //   micStatus=1;
       // Setvar(1);
        SendMessage('Experiment', 'ListenForMicAccess', 1);
        });

} else {
   console.log("getUserMedia not supported");
   micStatus=0;
}

},
CheckAssignmentID:function()
{
        var prolificID = document.getElementById('prolific_pid').value + ";";
        var studyID = "study" + document.getElementById('study_id').value + ";";
        var sessionID = "session" + document.getElementById('session_id').value;
        var combined = prolificID+studyID+sessionID;
        SendMessage('Experiment', 'ListenForAssignmentID', combined);
},

RecordStart: function(audioFileName)
{
    console.log("audiofile " + audioFileName);
    filename = Pointer_stringify(audioFileName);
    console.log(filename);
console.log("audio filename is: " + filename);
    chunks=[];
    console.log("starting recording");
 navigator.mediaDevices.getUserMedia({
            audio: true
        }).then(function(stream) {
            gumStream = stream;
            recorder = new MediaRecorder(stream);
            recorder.ondataavailable = function(e) {
                var url = URL.createObjectURL(e.data);
             //   var preview = document.createElement('audio');
              //  preview.controls = true;
               // preview.src = url;
                //document.body.appendChild(preview);
                console.log("about to ")
                chunks.push(e.data);
                   var blob = new Blob(chunks, {type: 'audio/ogg' }), bloburl = url;
  //                  , li = document.createElement('li'), mt = document.createElement('audio')
  //                 , hf = document.createElement('a');
  // mt.controls = true;
  // mt.src = bloburl;
  // hf.href = bloburl;
  // var filename = new Date().toISOString();
//   var tempFile = audioFileName;
//   console.log(audioFileName);
// var filename = Pointer_stringify(tempFile);

  var xhr=new XMLHttpRequest();
          xhr.onload=function(e) {
              if(this.readyState === 4) {
                  console.log("Server returned: ",e.target.responseText);
              }
          };
          var fd=new FormData();
          fd.append("functionname","PutObject");
          fd.append("subjectName",subject);
          fd.append("file",blob, filename);
          xhr.open("POST","functions.php",true);
          xhr.send(fd);

            };
            recorder.start();
        });
},
RecordStop: function()
{

    console.log("stopping recording");
        recorder.stop();
        gumStream.getAudioTracks()[0].stop();
},


requestFullScreen: function(element) {
    // Supports most browsers and their versions.
    var requestMethod = element.requestFullScreen || element.webkitRequestFullScreen || element.mozRequestFullScreen || element.msRequestFullscreen;

    if (requestMethod) { // Native full screen.
        requestMethod.call(element);
    } else if (typeof window.ActiveXObject !== "undefined") { // Older IE.
        var wscript = new ActiveXObject("WScript.Shell");
        if (wscript !== null) {
            wscript.SendKeys("{F11}");
        }
    }
},

makeFullScreen: function() {
if(document.getElementsByTagName("iframe") > 0)
{
    document.getElementsByTagName("iframe")[0].className = "fullScreen";
    var elem = document.body;
    requestFullScreen(elem);
}
},

submitForm: function(){
          document.forms["mturk_form"].submit();
        },

};


mergeInto(LibraryManager.library, BrowserPlugin);