namespace ManutencaoFrotaAPI.Validators
{
    using FluentValidation;
    using ManutencaoFrotaAPI.DTOs.Requests;

    public class ValidadorRequisicaoAtualizarProprietario : AbstractValidator<RequisicaoAtualizarProprietario>
    {
        public ValidadorRequisicaoAtualizarProprietario()
        {
            RuleFor(x => x.ProprietarioID)
                .GreaterThan(0).WithMessage("O campo ProprietarioID deve ser maior que zero.");
        }
    }
}
