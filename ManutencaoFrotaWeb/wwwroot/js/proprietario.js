$(document).ready(function () {
    // Função para carregar a lista de proprietários
    function carregarProprietarios() {
        $.ajax({
            url: 'https://localhost:7289/api/Proprietario/Listar',
            method: 'GET',
            headers: {
                'Authorization': 'Bearer ' + localStorage.getItem('jwtToken')
            },
            success: function (data) {
                if (data.length > 0) {
                    // Limpa o conteúdo existente na tabela
                    $('#proprietariosTabela tbody').empty();

                    // Processa e insere os proprietários na tabela
                    data.forEach(function (proprietario) {
                        var row = '<tr>' +
                            '<td>' + proprietario.nome + '</td>' +
                            '<td>' + proprietario.documento + '</td>' +
                            '</tr>';
                        $('#proprietariosTabela tbody').append(row);
                    });
                } else {
                    alert('Nenhum proprietário encontrado.');
                }
            },
            error: function (xhr, status, error) {
                $('#errorMessage').show(); // Exibe a mensagem de erro
                console.error('Erro na requisição:', status, error);
            }
        });
    }

    // Carrega a lista de proprietários ao carregar a página
    carregarProprietarios();
});

