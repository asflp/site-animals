let role;
let nickname;

$.get('http://localhost:5000/get-cookies', "json")
    .done(function (response) {
        let responseJson = JSON.parse(response);
        role = responseJson.Role;
        nickname = responseJson.User.Nickname;
    });

const enterButton = document.getElementById("enter_button");
enterButton.onclick = function () {
    if(role === "anonim") {
        window.location.href = 'http://localhost:5000/signin';
    } else {
        let url = new URL(`http://localhost:5000/profile`);
        url.searchParams.set('profile-id', nickname);
        window.location = url;
    }
}

const enterButton2 = document.getElementById("enter_button2");
enterButton2.onclick = function () {
    if(role === "anonim") {
        window.location.href = 'http://localhost:5000/signin';
    } else {
        let url = new URL(`http://localhost:5000/profile`);
        url.searchParams.set('profile-id', nickname);
        window.location = url;
    }
}

const profileDiv = document.getElementById("profile_div");
profileDiv.onclick = function () {
    if(role === "anonim") {
        window.location.href = 'http://localhost:5000/signin';
    } else {
        let url = new URL(`http://localhost:5000/profile`);
        url.searchParams.set('profile-id', nickname);
        window.location = url;
    }
}

document.querySelector('#forum_div').onclick = function () {
    window.location.href = 'http://localhost:5000/all-questions';
}

