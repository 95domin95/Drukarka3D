window.addEventListener("load", function () {

    var loginIcon = document.getElementById("login-icon");

    var loginForm = document.getElementById("login-form");

    loginIcon.addEventListener("click", function () {

        var loginIcon = document.getElementById("login-icon");

        loginIcon.style.visibility = "hidden";

        loginIcon.style.display = "none";

        var loginForm = document.getElementById("login-form");

        loginForm.style.visibility = "visible";

        loginForm.style.display = "block";

    });

    loginForm.addEventListener("click", function () {

        var loginForm = document.getElementById("login-form");

        loginForm.style.visibility = "hidden";

        loginForm.style.display = "none";

        var loginIcon = document.getElementById("login-icon");

        loginIcon.style.visibility = "visible";

        loginIcon.style.display = "block";

    });
}, false);