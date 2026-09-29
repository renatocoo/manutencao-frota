namespace ManutencaoFrotaAPI.DTOs.Requests
{
    public class RequisicaoAutenticacao
    {
        /// <summary>
        /// Nome do usuário
        /// </summary>
        public string Usuario { get; set; }

        /// <summary>
        /// Senha do usuário
        /// </summary>
        public string Senha { get; set; }
    }
}
