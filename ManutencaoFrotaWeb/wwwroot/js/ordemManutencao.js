$(document).ready(function () {
    console.log("Script de ordens de manutenção carregado com sucesso!"); // Mostra que o script foi carregado.

    // Função para selecionar/desselecionar todas as checkboxes habilitadas
    $('#selecionaTodas').change(function () {
        var isChecked = $(this).is(':checked'); // Verifica se a checkbox no cabeçalho está marcada
        $('.ordemCheckbox:enabled').prop('checked', isChecked); // Marca/desmarca apenas as checkboxes habilitadas
    });

    function carregarOrdens() { // Função para carregar as ordens
        const placa = $('#placa').val();
        const responsavelOrdem = $('#responsavelOrdem').val();
        const proprietario = $('#proprietario').val();

        const queryParams = $.param({
            placa: placa,
            responsavelOrdem: responsavelOrdem,
            proprietario: proprietario
        }); // Junta essas infos num formato para passar na URL

        $.ajax({
            url: 'https://localhost:7289/api/OrdemManutencao/Listar?' + queryParams, // URL para listar as ordens
            method: 'GET',
            headers: {
                'Authorization': 'Bearer ' + localStorage.getItem('jwtToken') // Usa o token guardado
            },
            success: function (data) {
                console.log("Resposta da API:", data); // Log para verificar erro

                if (Array.isArray(data) && data.length > 0) { // Se tiver ordens
                    console.log("Ordens de manutenção carregadas com sucesso.");
                    $('#ordensTabela tbody').empty();

                    const ultimasOrdensEmAberto = {}; // Guarda as últimas ordens abertas

                    data.forEach(function (ordem) { // Verifica se tem ordens abertas
                        if (ordem.status === 'EmAberto') {
                            ultimasOrdensEmAberto[ordem.veiculoPlaca] = ordem;
                        }
                    });

                    data.forEach(function (ordem) { // Para cada ordem...
                        var ultimaEmAberto = ultimasOrdensEmAberto[ordem.veiculoPlaca] && ultimasOrdensEmAberto[ordem.veiculoPlaca].id === ordem.id;

                        var dataFinalizacao = ordem.dataFinalizacao;
                        if (dataFinalizacao && !isNaN(Date.parse(dataFinalizacao))) {
                            dataFinalizacao = new Date(dataFinalizacao).toLocaleDateString(); // arruma formato de data
                        }

                        // Define se a checkbox deve estar habilitada ou desabilitada
                        var isCheckboxEnabled = ultimaEmAberto ? '' : 'disabled';

                        var row = '<tr>' +
                            '<td><input type="checkbox" class="ordemCheckbox" data-status="' + ordem.status + '" data-id="' + ordem.id + '" value="' + ordem.veiculoPlaca + '" ' + isCheckboxEnabled + '></td>' +
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

                    // Aplica um estilo para as checkboxes desabilitadas
                    $('.ordemCheckbox:disabled').parent().addClass('checkbox-disabled'); // Adiciona a classe para checkboxes desabilitadas
                } else {
                    alert('Nenhuma ordem encontrada.');
                }
            },
            error: function (xhr, status, error) {
                $('#erroCarregarOrdens').show(); // Mostra erro se a requisição falhar
                console.error('Erro na requisição:', status, error); // Log de verificacao
            }
        });
    }

    carregarOrdens(); // Carrega as ordens quando a página abre

    $('#filtrosForm').submit(function (e) {
        e.preventDefault(); // Não deixa o formulário recarregar a página
        carregarOrdens(); // Carrega as ordens filtradas
    });

    $('#cancelarOrdensButton').click(function () {
        let placas = getSelectedPlacas(); // Pega as placas selecionadas
        if (placas.length > 0) {
            placas.forEach(function (placa) {
                $.ajax({
                    url: 'https://localhost:7289/api/OrdemManutencao/Cancelar', // URL para cancelar
                    method: 'PATCH',
                    headers: {
                        'Authorization': 'Bearer ' + localStorage.getItem('jwtToken'),
                        'Content-Type': 'application/json'
                    },
                    data: JSON.stringify({ PlacaVeiculo: placa }), // Passa a placa
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
        let placas = getSelectedPlacas(); // Mesma coisa para encerrar
        if (placas.length > 0) {
            placas.forEach(function (placa) {
                $.ajax({
                    url: 'https://localhost:7289/api/OrdemManutencao/Encerrar', // URL para encerrar
                    method: 'PATCH',
                    headers: {
                        'Authorization': 'Bearer ' + localStorage.getItem('jwtToken'),
                        'Content-Type': 'application/json'
                    },
                    data: JSON.stringify({ PlacaVeiculo: placa }), // Passa a placa
                    success: function () {
                        alert('Ordem com placa ' + placa + ' encerrada com sucesso!'); // Mensagem de sucesso
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
        let placas = getSelectedPlacas(); // Mesma lógica para editar
        if (placas.length > 0) {
            let listaPlacas = placas.map(placa => '<p>Placa: ' + placa + '</p>').join('');
            $('#placasSelecionadas').html(listaPlacas); // Mostra as placas selecionadas
            $('#editarOrdemModal').modal('show'); // Mostra o modal de edição
        } else {
            alert('Nenhuma ordem selecionada.');
        }
    });

    $('#editarOrdemForm').submit(function (e) {
        e.preventDefault(); // Não deixa a página recarregar

        let placas = getSelectedPlacas();
        const quilometragemManutencao = $('#quilometragemManutencaoEditar').val(); // Pega a nova km

        if (placas.length > 0) {
            let promises = placas.map(function (placa) {
                return $.ajax({
                    url: `https://localhost:7289/api/OrdemManutencao/Atualizar/${placa}`, // URL para atualizar
                    method: 'PATCH',
                    headers: {
                        'Authorization': 'Bearer ' + localStorage.getItem('jwtToken'),
                        'Content-Type': 'application/json'
                    },
                    data: JSON.stringify({
                        KmManutencao: quilometragemManutencao // Envia a nova km
                    }),
                    success: function () {
                        console.log('Ordem com placa ' + placa + ' atualizada com sucesso!');
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        alert(`${jqXHR.responseText}`); // Mostra o erro
                    }
                });
            });

            Promise.all(promises)
                .then(function () {
                    alert('Todas as ordens selecionadas foram atualizadas com sucesso!');
                    $('#editarOrdemModal').modal('hide'); // Fecha o modal
                    carregarOrdens(); // Recarrega as ordens
                })
        } else {
            alert('Nenhuma ordem selecionada.'); // Aviso se nada foi escolhido
        }
    });

    $('#incluirOrdemModal').on('show.bs.modal', function () {
        $.ajax({
            url: 'https://localhost:7289/api/Veiculo/Listar', // Lista os veículos
            method: 'GET',
            headers: {
                'Authorization': 'Bearer ' + localStorage.getItem('jwtToken')
            },
            success: function (data) {
                if (Array.isArray(data) && data.length > 0) {
                    $('#placaVeiculo').empty().append('<option value="">Selecione um veículo</option>'); // Limpa e adiciona a opção
                    data.forEach(function (veiculo) {
                        $('#placaVeiculo').append('<option value="' + veiculo.placa + '">' + veiculo.placa + '</option>'); // Adiciona as placas na lista
                    });
                } else {
                    alert('Nenhum veículo encontrado.');
                }
            },
            error: function (xhr, status, error) {
                console.error('Erro ao carregar placas de veículos:', status, error); // Mostra erro
            }
        });
    });

    $('#incluirOrdemForm').submit(function (e) {
        e.preventDefault();

        const placaVeiculo = $('#placaVeiculo').val(); // Pega a placa do veículo
        const quilometragemManutencao = $('#quilometragemManutencao').val(); // Pega a km da manutenção

        if (placaVeiculo === "") {
            alert("Por favor, selecione um veículo.");
            return;
        }

        $.ajax({
            url: 'https://localhost:7289/api/OrdemManutencao/Incluir', // URL para incluir nova ordem
            method: 'POST',
            headers: {
                'Authorization': 'Bearer ' + localStorage.getItem('jwtToken'),
                'Content-Type': 'application/json'
            },
            data: JSON.stringify({
                PlacaVeiculo: placaVeiculo,
                KmManutencao: quilometragemManutencao // Envia a km
            }),
            success: function (data) {
                alert('Ordem de manutenção incluída com sucesso!');
                $('#incluirOrdemModal').modal('hide'); // Fecha o modal
                $('body').removeClass('modal-open');
                $('.modal-backdrop').remove();
                carregarOrdens();
                console.log("Resposta da API:", data);
            },
            error: function (jqXHR, errorThrown) {
                alert(jqXHR.responseText); // Mostra erro
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
            url: 'https://localhost:7289/api/OrdemManutencao/Exportar?' + queryParams, // URL para exportar
            method: 'GET',
            headers: {
                'Authorization': 'Bearer ' + localStorage.getItem('jwtToken')
            },
            success: function (response) {
                const { base64, nome, tipo } = response;

                const byteCharacters = atob(base64);
                const byteNumbers = new Array(byteCharacters.length);
                for (let i = 0; i < byteCharacters.length; i++) {
                    byteNumbers[i] = byteCharacters.charCodeAt(i); // Converte os dados
                }
                const byteArray = new Uint8Array(byteNumbers);

                const blob = new Blob([byteArray], { type: tipo }); // Cria o arquivo para download

                const link = document.createElement('a');
                link.href = window.URL.createObjectURL(blob); // Prepara o download
                link.download = nome; // Define o nome do arquivo
                link.click(); // Faz o download
                window.URL.revokeObjectURL(link.href); // Remove a URL temporária
            },
            error: function (xhr, status, error) {
                console.error('Erro ao exportar ordens de manutenção:', status, error); // Mostra erro
            }
        });
    });

    function getSelectedPlacas() { // Função para pegar as placas selecionadas
        let placas = [];
        $('.ordemCheckbox:checked').each(function () { // Pega as placas marcadas
            placas.push($(this).val());
        });
        return placas; // Retorna as placas
    }
});
