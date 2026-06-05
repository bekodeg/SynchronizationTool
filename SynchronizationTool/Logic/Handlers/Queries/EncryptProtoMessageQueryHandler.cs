using Google.Protobuf;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SynchronizationTool.Configuration;
using SynchronizationTool.Logic.Models;
using SynchronizationTool.Logic.Models.Queries;
using System.Security.Cryptography;
using System.Text;

namespace SynchronizationTool.Logic.Handlers.Queries
{
    public class EncryptProtoMessageQueryHandler(
        ILogger<EncryptProtoMessageQueryHandler> logger,
        IOptions<SecurityConfiguration> securityOptions) 
        : AbstractQueryHandler<EncryptProtoMessageQuery, byte[]>(logger)
    {
        private readonly SecurityConfiguration _securityConfiguration = securityOptions.Value;

        public override Task<ResponseModel<byte[]>> HandleAsync(
            EncryptProtoMessageQuery request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request.Message);

            var key = Encoding.UTF8.GetBytes(_securityConfiguration.Key);
            ArgumentNullException.ThrowIfNull(key);
            if (key.Length != 32) throw new ArgumentException("Key must be 256 bits (32 bytes)", nameof(key));

            byte[] plaintext = request.Message.ToByteArray();

            byte[] nonce = RandomNumberGenerator.GetBytes(12); // 96 бит
            byte[] ciphertext = new byte[plaintext.Length];
            byte[] tag = new byte[16];

            using var aes = new AesGcm(key, 16);
            aes.Encrypt(nonce, plaintext, ciphertext, tag);

            // Объединяем nonce + ciphertext + tag в один массив
            byte[] result = new byte[nonce.Length + ciphertext.Length + tag.Length];
            Buffer.BlockCopy(nonce, 0, result, 0, nonce.Length);
            Buffer.BlockCopy(ciphertext, 0, result, nonce.Length, ciphertext.Length);
            Buffer.BlockCopy(tag, 0, result, nonce.Length + ciphertext.Length, tag.Length);
            return Task.FromResult(new ResponseModel<byte[]>
            {
                Response = result,
            });
        }
    }
}
