window.addEventListener("load", function () {
    "use strict";

    $("#view").css("width", function () {
        return $(window).width() * 0.7;
    });
    $("#view").css("height", function () {
        return $(window).width() * 0.4;
    });

    var w = $(window).width() * 0.7, h = $(window).width() * 0.4;
    
    var renderer = new THREE.WebGLRenderer({
        preserveDrawingBuffer: true
    });

    renderer.setSize(w, h);
    var view = document.getElementById("view");
    view.appendChild(renderer.domElement);
    
    var camera = new THREE.PerspectiveCamera(45, w / h, 1, 1000);
    camera.position.set(0, 0, 50);
    var controls = new THREE.TrackballControls(camera, view);
    
    var scene = new THREE.Scene();
    scene.add(new THREE.AmbientLight(0x666666));
    
    var light1 = new THREE.DirectionalLight(0xffffff);
    light1.position.set(0, 100, 100);
    scene.add(light1);
    
    var light2 = new THREE.DirectionalLight(0xffffff);
    light2.position.set(0, -100, -100);
    scene.add(light2);
    
    var mat = new THREE.MeshPhongMaterial({
        color: 0x339900, ambient: 0x339900, specular: 0x030303
    });
    var obj = new THREE.Mesh(new THREE.Geometry(), mat);
    scene.add(obj);
    
    var loop = function loop() {
        requestAnimationFrame(loop);
        //obj.rotation.z += 0.05;
        controls.update();
        renderer.clear();
        renderer.render(scene, camera);
    };
    loop();
    
    // file load
    var openFile = function (file) {
        var reader = new FileReader();
        reader.addEventListener("load", function (ev) {
            var buffer = ev.target.result;
            var geom = loadStl(buffer);
            scene.remove(obj);
            obj = new THREE.Mesh(geom, mat);
            scene.add(obj);

        }, false);
        reader.readAsArrayBuffer(file);
    };

    var confirm = document.getElementById("confirm");

    confirm.addEventListener("click", function () {


        if (formValid()) {
            confirm.innerHTML = "Czekaj...";
            createCanvasScreenshot(view);
        }

    });

    var screenShotBtn = document.getElementById("screenShotBtn");

     var input = document.getElementById("file");
     input.addEventListener("change", function (ev) {
         var file = ev.target.files[0];
         if (checkForValidFileExtension(file.name)) openFile(file);
         else alert("Plik musi mieć rozszerzenie stl.");
     }, false);


}, false);

function checkForValidFileExtension(elemVal) 
{
    var fp = elemVal;
    if (fp.indexOf('.') === -1)
        return false;
 
    var allowedExts = new Array("stl");
    var ext = fp.substring(fp.lastIndexOf('.') + 1).
        toLowerCase();
 
    for (var i = 0; i < allowedExts.length; i++) 
	{
        if (ext === allowedExts[i]) return true;
    }
 
    return false;
}

function formValid()
{
    var projectName = $("#projectName").val();

    var validator = true;

    var alphNum = /^[a-zA-Z0-9]+$/;

    validator = alphNum.test(projectName) && projectName.length > 1;

    return validator;
}

function createCanvasScreenshot(canvas)
{
    var _url = 'UploadImage';

    var projectName = $("#projectName").val();

    var obj = document.querySelector('#view > canvas:nth-child(1)').toDataURL("image/png");

    obj = obj.replace('data:image/png;base64,', '');

    var screen = { "File": obj, "ProjectName": projectName};

    if (obj != null || obj != "") {
        $.ajax({
            method: "POST",
            url: _url,
            data: JSON.stringify(screen),
            dataType: 'JSON',
            contentType: "application/json; charset=utf-8"
        });
    }
    else {
        alert("Nie wybrano pliku");
        document.getElementById("confirm").innerHTML = "Drukuj";
    }


}
