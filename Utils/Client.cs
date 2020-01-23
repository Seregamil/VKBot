using VkNet.Model;

namespace VK_bot.Utils
{
    public class Client
    {
        private long id;
        private User handle;
        private string name;

        public Client(User handle) {
            this.handle = handle;
            id = this.handle.Id;

            name = $"{handle.FirstName} {handle.LastName}";
        }

        public long Id => this.id;
        public User Handle => handle;
        public string Name => name;
    }
}