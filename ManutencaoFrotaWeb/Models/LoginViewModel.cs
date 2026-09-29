namespace ManutencaoFrotaWeb.Models
{
    public class LoginViewModel
    {
        public string Usuario { get; set; }
        public string Senha { get; set; }
    }

    public class LoginResponseDto
    {
        public string Token { get; set; }
    }
}
