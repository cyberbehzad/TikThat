using CommunityToolkit.Mvvm.Messaging.Messages;
using TikThat.Models;

namespace TikThat.Messages;

public class CreateSampleTaskMessage(TodoItem value) : ValueChangedMessage<TodoItem>(value)
{
}
