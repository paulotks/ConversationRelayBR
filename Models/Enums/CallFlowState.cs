namespace ConversartionRelayBR.Models.Enums
{
    public enum CallFlowState
    {
        Initial,           // Aguardando primeira resposta
        SecondChance,      // Dando segunda chance 
        ShowingOptions,    // Mostrando opções DTMF
        WaitingDTMF,      // Aguardando tecla
        ShowingCompanyInfo, // Mostrando informações da empresa
        TransferToHuman   // Transferindo para humano
    }
}
