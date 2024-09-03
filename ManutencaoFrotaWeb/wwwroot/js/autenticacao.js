$(document).ready(function () {
    $('#loginForm').submit(function (e) {
        e.preventDefault();

        const loginData = {
            usuario: $('#username').val(),
            senha: $('#password').val()
        };

        fetch('https://localhost:7289/api/Autenticacao/Login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(loginData)
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error('Erro ao realizar login');
                }
                return response.json();
            })
            .then(data => {
                if (data.token) {
                    // Armazena o token JWT no localStorage
                    localStorage.setItem('jwtToken', data.token);
                    // Redireciona para a página principal ou qualquer outra página após o login
                    window.location.href = '/Home/Index';
                } else {
                    throw new Error('Token não recebido');
                }
            })
            .catch(error => {
                $('#loginError').text(error.message).show();
            });
    });

    // Verifica se o usuário já está logado ao carregar a página
    if (localStorage.getItem('jwtToken')) {
        window.location.href = '/Home/Index';
    }
});
