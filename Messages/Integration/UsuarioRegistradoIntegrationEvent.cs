namespace Messages.Integration
{
    public class UsuarioRegistradoIntegrationEvent : IntegrationEvent
    {
        public string Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }

        public override string Exchange => throw new NotImplementedException();

        public override string RoutingKey => throw new NotImplementedException();
    }
}
