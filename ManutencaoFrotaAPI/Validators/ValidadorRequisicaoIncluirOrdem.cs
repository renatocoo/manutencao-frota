namespace ManutencaoFrotaAPI.Validators
{
    using FluentValidation;
    using ManutencaoFrotaAPI.DTOs.Requests;

    public class ValidadorRequisicaoIncluirOrdem : AbstractValidator<RequisicaoIncluirOrdem>
    {
        public ValidadorRequisicaoIncluirOrdem()
        {
            RuleFor(x => x.PlacaVeiculo)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("O campo placaVeiculo é obrigatório.")
                .Matches("^[A-Z]{3}[0-9][A-Z][0-9]{2}$|^[A-Z]{3}[0-9]{4}$").WithMessage("Por favor, insira uma placa válida no padrão brasileiro.");

            RuleFor(x => x.KmManutencao)
                .GreaterThan(0).WithMessage("O campo KmManutencao deve ser maior que zero.");
        }
    }
}
