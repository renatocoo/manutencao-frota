$(document).ready(function () {
    console.log("Script de ordens de manutenção carregado com sucesso!");

    function carregarOrdens() {
        const placa = $('#placa').val();
        const responsavelOrdem = $('#responsavelOrdem').val();
        const proprietario = $('#proprietario').val();

        const queryParams = $.param({
            placa: placa,
            responsavelOrdem: responsavelOrdem,
            proprietario: proprietario
        });

        $.ajax({
            url: 'https://localhost:7289/api/OrdemManutencao/Listar?' + queryParams,
            method: 'GET',
            headers: {
                'Authorization': 'Bearer ' + localStorage.getItem('jwtToken')
            },
            success: function (data) {
                console.log("Resposta da API:", data);

                if (Array.isArray(data) && data.length > 0) {
                    console.log("Ordens de manutenção carregadas com sucesso.");
                    $('#ordensTabela tbody').empty();

                    const ultimasOrdensEmAberto = {};

                    data.forEach(function (ordem) {
                        if (ordem.status === 'EmAberto') {
                            ultimasOrdensEmAberto[ordem.veiculoPlaca] = ordem;
                        }
                    });

                    data.forEach(function (ordem) {
                        var isUltimaEmAberto = ultimasOrdensEmAberto[ordem.veiculoPlaca] && ultimasOrdensEmAberto[ordem.veiculoPlaca].id === ordem.id;

                        // Verifica e corrige a data de finalização, caso necessário
                        var dataFinalizacao = ordem.dataFinalizacao;
                        if (dataFinalizacao && !isNaN(Date.parse(dataFinalizacao))) {
                            dataFinalizacao = new Date(dataFinalizacao).toLocaleDateString();
                        }

                        var row = '<tr>' +
                            '<td><input type="checkbox" class="ordemCheckbox" data-status="' + ordem.status + '" data-id="' + ordem.id + '" value="' + ordem.veiculoPlaca + '" ' + (isUltimaEmAberto ? '' : 'disabled') + '></td>' +
                            '<td>' + ordem.veiculoPlaca + '</td>' +
                            '<td>' + ordem.responsavelOrdem + '</td>' +
                            '<td>' + ordem.proprietarioAtual + '</td>' +
                            '<td>' + ordem.status + '</td>' +
                            '<td>' + new Date(ordem.dataOrdem).toLocaleDateString() + '</td>' +
                            '<td>' + (dataFinalizacao || '') + '</td>' +
                            '<td>' + ordem.quilometragemManutencao + '</td>' +
                            '</tr>';
                        $('#ordensTabela tbody').append(row);
                    });
                } else {
                    alert('Nenhuma ordem encontrada.');
                }
            },
            error: function (xhr, status, error) {
                $('#erroCarregarOrdens').show();
                console.error('Erro na requisição:', status, error);
            }
        });
    }

    carregarOrdens();

    $('#filtrosForm').submit(function (e) {
        e.preventDefault();
        carregarOrdens();
    });

    $('#cancelarOrdensButton').click(function () {
        let placas = getSelectedPlacas();
        if (placas.length > 0) {
            placas.forEach(function (placa) {
                $.ajax({
                    url: 'https://localhost:7289/api/OrdemManutencao/Cancelar',
                    method: 'PATCH',
                    headers: {
                        'Authorization': 'Bearer ' + localStorage.getItem('jwtToken'),
                        'Content-Type': 'application/json'
                    },
                    data: JSON.stringify({ PlacaVeiculo: placa }),
                    success: function () {
                        alert('Ordem com placa ' + placa + ' cancelada com sucesso!');
                        carregarOrdens();
                    },
                    error: function () {
                        alert('Erro ao cancelar a ordem com placa ' + placa + '.');
                    }
                });
            });
        } else {
            alert('Nenhuma ordem selecionada.');
        }
    });

    $('#encerrarOrdensButton').click(function () {
        let placas = getSelectedPlacas();
        if (placas.length > 0) {
            placas.forEach(function (placa) {
                $.ajax({
                    url: 'https://localhost:7289/api/OrdemManutencao/Encerrar',
                    method: 'PATCH',
                    headers: {
                        'Authorization': 'Bearer ' + localStorage.getItem('jwtToken'),
                        'Content-Type': 'application/json'
                    },
                    data: JSON.stringify({ PlacaVeiculo: placa }),
                    success: function () {
                        alert('Ordem com placa ' + placa + ' encerrada com sucesso!');
                        carregarOrdens();
                    },
                    error: function () {
                        alert('Erro ao encerrar a ordem com placa ' + placa + '.');
                    }
                });
            });
        } else {
            alert('Nenhuma ordem selecionada.');
        }
    });

    $('#editarOrdensButton').click(function () {
        let placas = getSelectedPlacas();
        if (placas.length > 0) {
            let listaPlacas = placas.map(placa => '<p>Placa: ' + placa + '</p>').join('');
            $('#placasSelecionadas').html(listaPlacas);

            $('#editarOrdemModal').modal('show');
        } else {
            alert('Nenhuma ordem selecionada.');
        }
    });

    $('#editarOrdemForm').submit(function (e) {
        e.preventDefault();

        let placas = getSelectedPlacas();
        const quilometragemManutencao = $('#quilometragemManutencaoEditar').val();

        if (placas.length > 0) {
            let promises = placas.map(function (placa) {
                return $.ajax({
                    url: `https://localhost:7289/api/OrdemManutencao/Atualizar/${placa}`,
                    method: 'PATCH',
                    headers: {
                        'Authorization': 'Bearer ' + localStorage.getItem('jwtToken'),
                        'Content-Type': 'application/json'
                    },
                    data: JSON.stringify({
                        KmManutencao: quilometragemManutencao
                    }),
                    success: function () {
                        console.log('Ordem com placa ' + placa + ' atualizada com sucesso!');
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        alert(`${jqXHR.responseText}`);
                    }
                });
            });

            Promise.all(promises)
                .then(function () {
                    alert('Todas as ordens selecionadas foram atualizadas com sucesso!');
                    $('#editarOrdemModal').modal('hide');
                    carregarOrdens();
                })
        } else {
            alert('Nenhuma ordem selecionada.');
        }
    });

    $('#incluirOrdemModal').on('show.bs.modal', function () {
        $.ajax({
            url: 'https://localhost:7289/api/Veiculo/Listar',
            method: 'GET',
            headers: {
                'Authorization': 'Bearer ' + localStorage.getItem('jwtToken')
            },
            success: function (data) {
                if (Array.isArray(data) && data.length > 0) {
                    $('#placaVeiculo').empty().append('<option value="">Selecione um veículo</option>');
                    data.forEach(function (veiculo) {
                        $('#placaVeiculo').append('<option value="' + veiculo.placa + '">' + veiculo.placa + '</option>');
                    });
                } else {
                    alert('Nenhum veículo encontrado.');
                }
            },
            error: function (xhr, status, error) {
                console.error('Erro ao carregar placas de veículos:', status, error);
            }
        });
    });

    $('#incluirOrdemForm').submit(function (e) {
        e.preventDefault();

        const placaVeiculo = $('#placaVeiculo').val();
        const quilometragemManutencao = $('#quilometragemManutencao').val();

        if (placaVeiculo === "") {
            alert("Por favor, selecione um veículo.");
            return;
        }

        $.ajax({
            url: 'https://localhost:7289/api/OrdemManutencao/Incluir',
            method: 'POST',
            headers: {
                'Authorization': 'Bearer ' + localStorage.getItem('jwtToken'),
                'Content-Type': 'application/json'
            },
            data: JSON.stringify({
                PlacaVeiculo: placaVeiculo,
                KmManutencao: quilometragemManutencao
            }),
            success: function (data) {
                alert('Ordem de manutenção incluída com sucesso!');
                $('#incluirOrdemModal').modal('hide');
                $('body').removeClass('modal-open');
                $('.modal-backdrop').remove();
                carregarOrdens();
                console.log("Resposta da API:", data);
            },
            error: function (jqXHR, errorThrown) {
                alert(jqXHR.responseText);
            }
        });
    });


    $('#exportarXlsxButton').click(function () {
        const placa = $('#placa').val();
        const responsavelOrdem = $('#responsavelOrdem').val();
        const proprietario = $('#proprietario').val();

        const queryParams = $.param({
            placa: placa,
            responsavelOrdem: responsavelOrdem,
            proprietario: proprietario
        });

        $.ajax({
            url: 'https://localhost:7289/api/OrdemManutencao/Exportar?' + queryParams,
            method: 'GET',
            headers: {
                'Authorization': 'Bearer ' + localStorage.getItem('jwtToken')
            },
            success: function (response) {
                const { base64, nome, tipo } = response;

                const byteCharacters = atob(base64);
                const byteNumbers = new Array(byteCharacters.length);
                for (let i = 0; i < byteCharacters.length; i++) {
                    byteNumbers[i] = byteCharacters.charCodeAt(i);
                }
                const byteArray = new Uint8Array(byteNumbers);

                const blob = new Blob([byteArray], { type: tipo });

                const link = document.createElement('a');
                link.href = window.URL.createObjectURL(blob);
                link.download = nome;
                link.click();
                window.URL.revokeObjectURL(link.href);
            },
            error: function (xhr, status, error) {
                console.error('Erro ao exportar ordens de manutenção:', status, error);
            }
        });
    });

    function getSelectedPlacas() {
        let placas = [];
        $('.ordemCheckbox:checked').each(function () {
            placas.push($(this).val());
        });
        return placas;
    }
});
