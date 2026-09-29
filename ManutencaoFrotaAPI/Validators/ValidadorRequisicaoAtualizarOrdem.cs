namespace ManutencaoFrotaAPI.Validators
{
    using FluentValidation;
    using ManutencaoFrotaAPI.DTOs.Requests;

    public class ValidadorRequisicaoAtualizarOrdem : AbstractValidator<RequisicaoAtualizarOrdem>
    {
        public ValidadorRequisicaoAtualizarOrdem()
        {
            RuleFor(x => x.KmManutencao)
                .GreaterThan(0).WithMessage("O campo KmManutencao deve ser maior que zero.");
        }
    }
}
