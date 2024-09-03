namespace ManutencaoFrotaAPI.Validators
{
    using FluentValidation;
    using ManutencaoFrotaAPI.DTOs.Requests;

    public class ValidadorRequisicaoOrdem : AbstractValidator<RequisicaoOrdem>
    {
        public ValidadorRequisicaoOrdem()
        {
            RuleFor(x => x.PlacaVeiculo)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("O campo placaVeiculo é obrigatório.")
                .Matches("^[A-Z]{3}[0-9][A-Z][0-9]{2}$|^[A-Z]{3}[0-9]{4}$").WithMessage("Por favor, insira uma placa válida no padrão brasileiro.");
        }
    }
}
