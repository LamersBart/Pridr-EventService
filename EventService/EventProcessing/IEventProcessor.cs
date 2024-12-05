namespace EventService.EventProcessing;

public interface IEventProcessor
{
    void ProcessEvent(string message);
}