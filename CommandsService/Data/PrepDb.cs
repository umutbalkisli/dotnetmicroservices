using System;
using System.Collections;
using System.Collections.Generic;
using CommandService.SyncDataServices.Grpc;
using CommandsService.Data;
using CommandsService.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace CommandService.Data
{
    public static class PrepDb
    {
        public static void PrepPopulation(IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                 var grpcClient = serviceScope.ServiceProvider.GetService<IPlatformDataClient>();

                 var platforms = grpcClient.ReturnAllPlatforms();

                 SeedData(serviceScope.ServiceProvider.GetService<ICommandRepo>(), platforms);
            }
        }

        private static void SeedData(ICommandRepo repo, IEnumerable<Platform> platforms)
        {
            Console.WriteLine("--> Seeding new platforms...");

            if (platforms == null)
            {
                Console.WriteLine("--> No platforms found to seed...");
                return;
            }

            foreach (var platform in platforms)
            {
                if (!repo.ExternalPlatformExists(platform.ExternalID))
                {
                    repo.CreatePlatform(platform);
                }
            }

            repo.SaveChanges();
        }
    }
}