using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

public class CertificatePinningEndpointBehavior : IEndpointBehavior
{
    private readonly Func<HttpMessageHandler> _httpHandler;

    public CertificatePinningEndpointBehavior(IHttpMessageHandlerFactory factory) 
    {
        _httpHandler = () => factory.CreateHandler();
    }

    public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
    {
        bindingParameters.Add(new Func<HttpClientHandler, HttpMessageHandler>(handler => _httpHandler()));
    }

    public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime) { }

    public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }

    public void Validate(ServiceEndpoint endpoint) { }
}

