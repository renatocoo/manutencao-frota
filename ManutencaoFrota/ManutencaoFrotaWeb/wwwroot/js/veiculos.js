$(document).ready(function () {
    console.log("Script de veículos carregado com sucesso!");

    // Carrega a lista de proprietários ao abrir a página
    let proprietariosDisponiveis = [];

    function carregarProprietarios() {
        $.ajax({
            url: 'https://localhost:7289/api/Proprietario/Listar',
            method: 'GET',
            headers: {
                'Authorization': 'Bearer ' + localStorage.getItem('jwtToken')
            },
            success: function (data) {
                proprietariosDisponiveis = data;
                $('#editarProprietario').empty();
                data.forEach(function (proprietario) {
                    $('#editarProprietario').append(
                        $('<option>', {
                            value: proprietario.id,
                            text: proprietario.nome
                        })
                    );
                });
            },
            error: function () {
                alert('Erro ao carregar os proprietários.');
            }
        });
    }

    // Chama a função para carregar os proprietários ao iniciar
    carregarProprietarios();

    // Função para carregar os veículos com filtros
    $('#filtrosForm').submit(function (e) {
        e.preventDefault();

        const placa = $('#placa').val();
        const proprietario = $('#proprietario').val();

        const queryParams = $.param({ placa: placa, proprietario: proprietario });

        $.ajax({
            url: 'https://localhost:7289/api/Veiculo/Listar?' + queryParams,
            method: 'GET',
            headers: {
                'Authorization': 'Bearer ' + localStorage.getItem('jwtToken')
            },
            success: function (data) {
                if (data.length > 0) {
                    console.log("Veículos carregados com sucesso.");
                    $('#veiculosTabela tbody').empty();

                    data.forEach(function (veiculo) {
                        var row = '<tr>' +
                            '<td><input type="checkbox" class="veiculoCheckbox" value="' + veiculo.placa + '"></td>' +
                            '<td>' + veiculo.placa + '</td>' +
                            '<td>' + veiculo.proprietario + '</td>' +
                            '<td>' + veiculo.documento + '</td>' +
                            '<td>' + veiculo.quilometragem + '</td>' +
                            '<td>' + veiculo.ultimaManutencao + '</td>' +
                            '</tr>';
                        $('#veiculosTabela tbody').append(row);
                    });

                } else {
                    alert('Nenhum veículo encontrado.');
                }
            },
            error: function (xhr, status, error) {
                $('#erroCarregarVeiculos').show();
                console.error('Erro na requisição:', status, error);
            }
        });
    });

    // Função para abrir o modal de edição de veículo
    function editarVeiculo(placas) {
        if (placas.length === 1) {
            const placa = placas[0];
            $.ajax({
                url: 'https://localhost:7289/api/Veiculo/Listar?placa=' + placa,
                method: 'GET',
                headers: {
                    'Authorization': 'Bearer ' + localStorage.getItem('jwtToken')
                },
                success: function (data) {
                    if (data.length > 0) {
                        var veiculo = data[0];
                        $('#veiculoId').val(veiculo.placa);
                        $('#editarPlaca').val(veiculo.placa);
                        $('#editarProprietario').val(veiculo.proprietarioID); // Seleciona o proprietário correto
                        $('#editarVeiculoModal').modal('show');
                    } else {
                        alert('Veículo não encontrado.');
                    }
                },
                error: function () {
                    alert('Erro ao carregar os dados do veículo.');
                }
            });
        } else {
            alert('Selecione apenas um veículo para editar.');
        }
    }

    // Função para salvar as alterações do veículo
    $('#editarVeiculoForm').submit(function (e) {
        e.preventDefault();

        var placa = $('#editarPlaca').val();
        var proprietarioID = $('#editarProprietario').val();

        $.ajax({
            url: 'https://localhost:7289/api/Veiculo/Atualizar/' + placa,
            method: 'PATCH',
            headers: {
                'Authorization': 'Bearer ' + localStorage.getItem('jwtToken'),
                'Content-Type': 'application/json'
            },
            data: JSON.stringify({
                ProprietarioID: proprietarioID
            }),
            success: function () {
                alert('Veículo atualizado com sucesso!');
                $('#editarVeiculoModal').modal('hide');
                $('#filtrosForm').submit();
            },
            error: function () {
                alert('Erro ao atualizar o veículo.');
            }
        });
    });

    // Evento do botão "Editar Selecionados"
    $('#editarSelecionadosButton').click(function () {
        const selectedPlacas = getSelectedPlacas();
        editarVeiculo(selectedPlacas);
    });

    // Função para obter as placas dos veículos selecionados
    function getSelectedPlacas() {
        let placas = [];
        $('.veiculoCheckbox:checked').each(function () {
            placas.push($(this).val());
        });
        return placas;
    }

    // Selecionar ou desmarcar todos os checkboxes
    $('#selectAll').change(function () {
        $('.veiculoCheckbox').prop('checked', $(this).prop('checked'));
    });

    // Carrega a lista de veículos ao carregar a página
    $('#filtrosForm').submit();
});
