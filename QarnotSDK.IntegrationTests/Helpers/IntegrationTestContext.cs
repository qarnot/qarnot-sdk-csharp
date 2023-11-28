using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using NUnit.Framework;
using Radosgw.AdminAPI;

namespace QarnotSDK.IntegrationTests
{
    public class IntegrationTestContext
    {
        private readonly List<(string tenant, string uid)> StorageUsersToKeep = new ()
        {
            ("rgw", "rgw"), // admin
        };

        public CephClientOptions CephOptions { get; set; }
        public RadosGWAdminConnection CephAdminClient { get; set; }

        public IntegrationTestContext(CephClientOptions options)
        {
            CephOptions = options;
            CreateCephConnection(options);
            var adminUserId = Environment.GetEnvironmentVariable("QARNOT_SDK_CSHARP_TESTS_STORAGE_ADMIN_UID") ?? "rgw";
            StorageUsersToKeep.Add((null, adminUserId));
        }

        public async Task ResetStorageAsync()
        {
            try
            {
                var useRemoteStorage = bool.Parse(
                    Environment.GetEnvironmentVariable("QARNOT_SDK_CSHARP_TESTS_USE_REAL_REMOTE_STORAGE") ?? "false");

                if (useRemoteStorage)
                {
                    var storageAccounts = await CephAdminClient.ListUsersAsync(timeout:TimeSpan.FromSeconds(20));
                    TestContext.Progress.WriteLine("storage accounts retrieved from ceph client {0}", string.Join(",", storageAccounts));
                    var storageAccountsIds = storageAccounts
                        ?.Where(account => !string.IsNullOrWhiteSpace(account))
                        ?.Select(account =>
                        {
                            if (account.Contains('$', StringComparison.CurrentCultureIgnoreCase))
                            {
                                var tenant = account.Split('$')[0];
                                var uid = account.Split('$')[1];
                                return (tenant, uid);
                            }
                            return (default, account);
                        })
                        ?? new List<(string, string)>();
                    TestContext.Progress.WriteLine("storage accounts ids: {0}", string.Join(",", storageAccountsIds.Select(x => $"[uuid:{x.uid}, tenant:{x.tenant}]")));

                    var storageAccountsToRemove = storageAccountsIds
                        .Where(acc => !StorageUsersToKeep
                            .Any(toKeep => toKeep.tenant == acc.tenant && toKeep.uid == acc.uid));

                    foreach((string accountTenant, string accountUid) in storageAccountsToRemove)
                    {
                        await CephAdminClient.RemoveUserAsync(accountTenant, accountUid, true, timeout:TimeSpan.FromSeconds(20));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"could not reset storage {ex.Message}");
            }
        }

        public async Task CreateStorageAccount(string storageId, string storageTenant, string name, string accessKey, string secretKey)
        {
            var retries = 0;
            var maxRetries = Environment.GetEnvironmentVariable("QARNOT_SDK_CSHARP_TESTS_RETRIES_COUNT") != default ?
                int.Parse(Environment.GetEnvironmentVariable("QARNOT_SDK_CSHARP_TESTS_RETRIES_COUNT"))
                : 20;
            var done = false;
            while (!done && retries <= maxRetries)
            {
                try
                {
                    var user = await CephAdminClient.CreateUserAsync(
                        uid: storageId,
                        displayName: name,
                        tenant: storageTenant,
                        keyType: "s3",
                        accessKey: accessKey,
                        secretKey: secretKey,
                        generateKey: false);

                    user = await CephAdminClient.ModifyUserAsync(uid:user.UserId, displayName:name, tenant:storageTenant, userCaps: "read,write,delete");
                }
                catch(Exception e)
                {
                    TestContext.Progress.WriteLine("Unexpected exception when creating ceph account. Will Try again ({0}/{1}). \n{2}", retries, maxRetries, e);
                }
                try
                {
                    var storageAccounts = await CephAdminClient.ListUsersAsync(timeout:TimeSpan.FromSeconds(20));
                    TestContext.Progress.WriteLine("storage accounts retrieved from ceph client {0}", string.Join(",", storageAccounts));
                    done = storageAccounts.Contains(string.Format("{0}${1}", storageTenant, storageId));
                }
                catch (Exception e)
                {
                    TestContext.Progress.WriteLine("Failed to list users: {0}", e);
                }
                if (!done) {
                    Thread.Sleep(400);
                }
                retries += 1;
            }
        }

        public AmazonS3Client GetUserStorageClient(string accessKey, string secretKey, HttpClientHandler httpClientHandler)
        {
            var config = new AmazonS3Config()
            {
                ServiceURL = CephOptions.RadosGWEndpoint,
                ForcePathStyle = true,
                MaxErrorRetry = 3
            };

            if (httpClientHandler != null) {
                var proxy = httpClientHandler.Proxy as System.Net.WebProxy;
                if (proxy != null) config.SetWebProxy(proxy);
                config.HttpClientFactory = new FakeS3HttpClientFactory(httpClientHandler);
            }

            return new AmazonS3Client(accessKey, secretKey, config);
        }

        private void CreateCephConnection(CephClientOptions options)
        {
            CephAdminClient = new RadosGWAdminConnection(
                options.RadosGWEndpoint,
                options.RadosGWAdminAccessKey,
                options.RadosGWAdminSecretKey,
                options.RadosGWAdminAdminPath);
        }

        public static void IgnoreIfNotRealStorage()
        {
            var useRealStorage = IsUsingRealStorage();
            if (!useRealStorage)
            {
                Assert.Ignore("Not using real storage. Omitting.");
            }
        }

        public static bool IsUsingRealStorage()
        {
            return bool.Parse(Environment.GetEnvironmentVariable("QARNOT_SDK_CSHARP_TESTS_USE_REAL_REMOTE_STORAGE") ?? "false");
        }
    }

    public static class Gitlabci
    {
        public static bool IsInGitlabCI()
        {
            return System.Environment.GetEnvironmentVariable("GITLAB_CI") == "true";
        }
    }
}
