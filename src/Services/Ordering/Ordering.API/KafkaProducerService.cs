using Confluent.Kafka;
using Microsoft.Extensions.Configuration;


namespace Ordering.API;

public interface IKafkaProducerService
{
    Task ProduceAsync(string message);
}

public class KafkaProducerService : IKafkaProducerService
{
    private readonly IProducer<string, string> _producer;
    private readonly string _topic;

    public KafkaProducerService(IConfiguration config)
    {
        var kafkaConfig = config.GetSection("Kafka");

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = kafkaConfig["BootstrapServers"],

                SecurityProtocol = SecurityProtocol.SaslPlaintext,

                // Auth
                SaslMechanism = SaslMechanism.ScramSha512,
            SaslUsername = kafkaConfig["Username"],
            SaslPassword = kafkaConfig["Password"],

            Acks = Acks.All
        };

        _producer = new ProducerBuilder<string, string>(producerConfig).Build();
        _topic = kafkaConfig["Topic"]!;
    }

    public async Task ProduceAsync(string message)
    {
        await _producer.ProduceAsync(
            _topic,
            new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = message
            });
    }
}
