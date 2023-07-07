using CryptoBank.Database;
using CryptoBank.Domain.Models;
using CryptoBank.WebAPI.Common.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NBitcoin;

namespace CryptoBank.WebAPI.Features.Deposits.Requests;

public class GetDepositAddress
{
    public record Request : IRequest<Response>;

    public record Response(string CryptoAddress);

    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly CurrentAuthInfoSource _currentAuthInfoSource;
        private readonly CryptoBankDbContext _dbContext;

        public RequestHandler(CryptoBankDbContext dbContext, CurrentAuthInfoSource currentAuthInfoSource)
        {
            _dbContext = dbContext;
            _currentAuthInfoSource = currentAuthInfoSource;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var (xpub, derivationIndex) = await GetXpubAndNextDerivationIndex(cancellationToken);

            var userCryptoAddress = CreateCryptoAddress(xpub.Value, derivationIndex);

            await SaveToDbDepositAddress(xpub.Id, derivationIndex, userCryptoAddress, cancellationToken);

            return new Response(userCryptoAddress);
        }

        private async Task<(Xpub xpub, uint nextDerivationIndex)> GetXpubAndNextDerivationIndex(
            CancellationToken cancellationToken)
        {
            var xpub = await _dbContext.Xpubs.SingleAsync(cancellationToken);

            var derivationIndex = xpub.LastUsedDerivationIndex + 1;

            //TODO: potential problem: guess race condition, if two users will try to get address at the same time
            await _dbContext.Xpubs
                .Where(x => x.Id == xpub.Id)
                .ExecuteUpdateAsync(
                    s => s.SetProperty(p => p.LastUsedDerivationIndex, derivationIndex),
                    cancellationToken);

            return (xpub, derivationIndex);
        }

        private static string CreateCryptoAddress(string base58ExtPubKey, uint derivationIndex)
        {
            //TODO: move network to config
            var network = Network.TestNet;
            var masterExtPubKey = new BitcoinExtPubKey(base58ExtPubKey, network).ExtPubKey;

            var userPubKey = masterExtPubKey.Derive(derivationIndex).PubKey;

            var bitcoinPubKeyAddress = userPubKey.Hash.GetAddress(network);

            var userCryptoAddress = bitcoinPubKeyAddress.ToString();
            return userCryptoAddress;
        }

        private async Task SaveToDbDepositAddress(
            int xpubId,
            uint derivationIndex,
            string cryptoAddress,
            CancellationToken cancellationToken)
        {
            var userId = _currentAuthInfoSource.GetUserId();

            var depositAddress = new DepositAddress("BTC", derivationIndex, cryptoAddress, userId, xpubId);

            await _dbContext.DepositAddresses.AddAsync(depositAddress, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
