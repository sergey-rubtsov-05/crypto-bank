using CryptoBank.Database;
using CryptoBank.Domain.Models;
using CryptoBank.WebAPI.Common.Errors.Exceptions;
using CryptoBank.WebAPI.Common.Services;
using CryptoBank.WebAPI.Features.Deposits.Errors;
using CryptoBank.WebAPI.Features.Deposits.Services;
using JetBrains.Annotations;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NBitcoin;

namespace CryptoBank.WebAPI.Features.Deposits.Requests;

public static class GetDepositAddress
{
    public record Request : IRequest<Response>;

    public record Response(string CryptoAddress);

    [UsedImplicitly]
    public class RequestHandler : IRequestHandler<Request, Response>
    {
        private readonly CurrentAuthInfoSource _currentAuthInfoSource;
        private readonly CryptoBankDbContext _dbContext;
        private readonly NetworkSource _networkSource;

        public RequestHandler(
            CurrentAuthInfoSource currentAuthInfoSource,
            CryptoBankDbContext dbContext,
            NetworkSource networkSource)
        {
            _currentAuthInfoSource = currentAuthInfoSource;
            _dbContext = dbContext;
            _networkSource = networkSource;
        }

        public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
        {
            var userId = _currentAuthInfoSource.GetUserId();

            var existingCryptoAddress = await GetExistingCryptoAddress(userId, cancellationToken);
            if (existingCryptoAddress is not null)
                return new Response(existingCryptoAddress);

            var userCryptoAddress = await CreateNewCryptoAddress(userId, cancellationToken);

            return new Response(userCryptoAddress);
        }

        private async Task<string?> GetExistingCryptoAddress(int userId, CancellationToken cancellationToken)
        {
            return await _dbContext.DepositAddresses
                .Where(address => address.UserId == userId)
                .Select(address => address.CryptoAddress)
                .SingleOrDefaultAsync(cancellationToken);
        }

        private async Task<string> CreateNewCryptoAddress(int userId, CancellationToken cancellationToken)
        {
            var (xpub, derivationIndex) = await GetXpubAndNextDerivationIndex(cancellationToken);

            var userCryptoAddress = CreateCryptoAddress(xpub.Value, derivationIndex);

            await SaveToDbDepositAddress(xpub.Id, derivationIndex, userCryptoAddress, userId, cancellationToken);
            return userCryptoAddress;
        }

        private async Task<(Xpub xpub, uint nextDerivationIndex)> GetXpubAndNextDerivationIndex(
            CancellationToken cancellationToken)
        {
            var xpub = await _dbContext.Xpubs.SingleOrDefaultAsync(cancellationToken);
            if (xpub is null)
                throw new LogicConflictException(DepositsLogicConflictError.ServiceIsNotConfigured);

            var derivationIndex = xpub.LastUsedDerivationIndex + 1;

            //TODO: potential problem: guess race condition, if two users will try to get address at the same time
            await _dbContext.Xpubs
                .Where(x => x.Id == xpub.Id)
                .ExecuteUpdateAsync(
                    s => s.SetProperty(p => p.LastUsedDerivationIndex, derivationIndex),
                    cancellationToken);

            return (xpub, derivationIndex);
        }

        private string CreateCryptoAddress(string base58ExtPubKey, uint derivationIndex)
        {
            var network = _networkSource.Get();
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
            int userId,
            CancellationToken cancellationToken)
        {
            var depositAddress = new DepositAddress("BTC", derivationIndex, cryptoAddress, userId, xpubId);

            await _dbContext.DepositAddresses.AddAsync(depositAddress, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
