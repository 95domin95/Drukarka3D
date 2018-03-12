window.addEventListener("load", function () 
{
    var self = this;
    self.handleIncomingFiles = function(e) 
	{
        var files = e.dataTransfer.files;
		
		//dla wielu plików
        for (var i=0; i<files.length; i++) 
		{
            var file = files[i];
			if(checkForValidFileExtension(file.name))
			{
				var reader = new FileReader();
				reader.addEventListener("loadend", function(file, e) 
				{
					var container = document.getElementById("uploader");
					container.innerHTML = "";
					var div = document.createElement("div");
					div.innerHTML = '<div class="droppedFile"><b>'
						+ file.name+" ("
						+ file.size+"B)</b><i>";
						// + this.result.substr(0, 100)+"</i></div>";
					container.appendChild(div);
				}.bind(reader, file));
				reader.readAsDataURL(file);
				draw3dScene(file);
			}
			else alert('Plik musi mieć rozszerzenie "obj"');
        }
    };
    var uploader = document.getElementById("uploader");
    uploader.addEventListener("dragover", function(e) 
	{
        e.preventDefault();
    });
    uploader.addEventListener("drop", function(e) 
	{
        e.preventDefault();
        self.handleIncomingFiles(e);
        uploader.style.backgroundColor = "";
    });
    uploader.addEventListener("dragenter", function(e) 
	{
        uploader.style.backgroundColor = "#ADF";
    });
    uploader.addEventListener("dragleave", function(e) 
	{
        uploader.style.backgroundColor = "";
    });
	
	var button = document.getElementById("confirm");
	button.addEventListener("click", function()
	{
		alert("Wysyłanie pliku...");
	});
}, false);

function draw3dScene(file)
{
	var scene = new THREE.Scene();
	var camera = new THREE.PerspectiveCamera(50, 500 / 400, 0.1, 1000);

	//var renderer = new THREE.WebGLRenderer();
	var renderer = new THREE.WebGLRenderer( { canvas: artifactCanvas } );
	renderer.setSize(500, 400);
	//document.body.appendChild(renderer.domElement);

	var geometry = new THREE.SphereGeometry(3, 50, 50, 0, Math.PI * 2, 0, Math.PI * 2);
	var material1 = new THREE.MeshBasicMaterial();
	var material2 = new THREE.MeshBasicMaterial();
	var sphere = [new THREE.Mesh(geometry, material1), new THREE.Mesh(geometry, material1), new THREE.Mesh(geometry, material2)];

	sphere[0].position.set(1, 1, 1);
	sphere[1].position.set(-1, -1, -1);

	scene.add(sphere[0]);
	scene.add(sphere[1]);
	scene.add(sphere[2]);

	camera.position.z = 10;


	var hex = "0x" + "000000".replace(/0/g, function() {
	  return (~~(Math.random() * 16)).toString(16);
	});
	sphere[0].material.color.setHex(hex);

	hex = "0x" + "000000".replace(/0/g, function() {
	  return (~~(Math.random() * 16)).toString(16);
	});
	sphere[2].material.color.setHex(hex);


	var render = function() {
	  requestAnimationFrame(render);
	  renderer.render(scene, camera);
	};

	render();
}

function checkForValidFileExtension(elemVal) 
{
    var fp = elemVal;
    if (fp.indexOf('.') == -1)
        return false;
 
    var allowedExts = new Array("obj");
    var ext = fp.substring(fp.lastIndexOf('.') + 1).
        toLowerCase();
 
    for (var i = 0; i < allowedExts.length; i++) 
	{
        if (ext == allowedExts[i]) return true;
    }
 
    return false;
}