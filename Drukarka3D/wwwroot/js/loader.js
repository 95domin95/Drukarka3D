window.addEventListener("load", function () {
    "use strict";
    
    var w = 640, h = 480;
    
    var renderer = new THREE.WebGLRenderer();
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

			//var container = document.getElementById("uploader");
			//container.innerHTML = "";
			//var div = document.createElement("div");
			//div.innerHTML = '<div class="droppedFile"><b>'
			//	+ file.name+" ("
			//	+ file.size+"B)</b><i>";
			//	// + this.result.substr(0, 100)+"</i></div>";
			//container.appendChild(div);

        }, false);
        reader.readAsArrayBuffer(file);
    };

    var confirm = document.getElementById("confirm");

    confirm.addEventListener("click", function () {
        confirm.innerHTML = "Czekaj..."
    });

     var input = document.getElementById("file");
     input.addEventListener("change", function (ev) {
         var file = ev.target.files[0];
         if (checkForValidFileExtension(file.name)) openFile(file);
         else alert("Plik musi mieć rozszerzenie stl.");
     }, false);

	//var uploader = document.getElementById("uploader");
 //   // dnd
 //   uploader.addEventListener("dragover", function (ev) {
 //       ev.stopPropagation();
 //       ev.preventDefault();
 //       ev.dataTransfer.dropEffect = "copy";
 //   }, false);
 //   uploader.addEventListener("drop", function (ev) {
 //       ev.stopPropagation();
 //       ev.preventDefault();
 //       var file = ev.dataTransfer.files[0];
 //       if(checkForValidFileExtension(file.name)) openFile(file);
	//	else alert("Plik musi mieć rozszerzenie stl.");
 //   }, false);

    //confirm.addEventListener("click", function () {
    //    var inputfile = document.getElementById(filename);
    //    var files = inputfile.files;
    //    var 

    //});

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
