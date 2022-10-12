using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using MediatR;
using Spine.Core.Invoices.Commands;

namespace Spine.Core.Invoices.Jobs
{
    public class MediatorSerializedObject
    {
        public string FullTypeName { get; private set; }

        public string Data { get; private set; }

        public string AdditionalDescription { get; private set; }

        public MediatorSerializedObject(string fullTypeName, string data, string additionalDescription)
        {
            this.FullTypeName = fullTypeName;
            this.Data = data;
            this.AdditionalDescription = additionalDescription;
        }

        /// <summary>
        /// Override for Hangfire dashboard display.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var commandName = this.FullTypeName.Split('.').Last();
            return $"{commandName} {this.AdditionalDescription}";
        }
    }

    public class CommandsExecutor
    {
        private readonly IMediator mediator;
        public CommandsExecutor(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [DisplayName("Processing command {0}")]
        public Task ExecuteCommand(MediatorSerializedObject mediatorSerializedObject)
        {
            var type = Assembly.GetAssembly(typeof(CreateRecurringInvoiceCommand)).GetType(mediatorSerializedObject.FullTypeName);

            if (type != null)
            {
                dynamic req = JsonSerializer.Deserialize(mediatorSerializedObject.Data, type);

                return this.mediator.Send(req as IRequest);
            }

            return null;
        }
    }
}
