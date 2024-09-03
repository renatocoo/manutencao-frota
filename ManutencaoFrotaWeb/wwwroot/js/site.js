// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function verificarAutenticacao() {
    const jwtToken = localStorage.getItem('jwtToken');
    if (!jwtToken) {
        // Redireciona para a página de login se nao estivar com o token JWT
        window.location.replace('/Autenticacao/Login');
    }
}

// Roda a função
verificarAutenticacao();

//Botao de Login ou Logout
$(document).ready(function () {
    const jwtToken = localStorage.getItem('jwtToken');

    if (jwtToken) {
        // Decodifica o token JWT
        const payload = JSON.parse(atob(jwtToken.split('.')[1]));
        const userName = payload.sub;  // `sub` é o nome de usuário

        // Exibe o nome do usuário e o botão de logout
        $('#userName').text(`Bem-vindo, ${userName}`);
        $('#userInfo').show();
        $('#logoutMenuItem').show();
        $('#loginMenuItem').hide();

        // Configura o botão de logout
        $('#logoutButton').on('click', function () {
            localStorage.removeItem('jwtToken');
            window.location.href = '/Autenticacao/Login';
        });
    }
});
