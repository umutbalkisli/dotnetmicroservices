using System;
using System.Text.Json;
using AutoMapper;
using CommandsService.Data;
using CommandsService.Dtos;
using CommandsService.Models;
using Microsoft.Extensions.DependencyInjection;

namespace CommandsService.EventProcessing
{
    public class EventProcessor : IEventProcessor
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly IMapper mapper;

        public EventProcessor(IServiceScopeFactory scopeFactory, IMapper mapper)
        {
            this.scopeFactory = scopeFactory;
            this.mapper = mapper;
        }

        public void ProcessEvent(string message)
        {
            var eventType = DetermineEvent(message);

            switch (eventType)
            {
                case EventType.PlatformPublished:
                    Console.WriteLine("--> EventType.PlatformPublished");
                    AddPlatform(message);
                    break;
                case EventType.Undetermined:
                    Console.WriteLine("--> ");
                    break;
                default:
                    Console.WriteLine("--> ");
                    break;
            }
        }

        public EventType DetermineEvent(string notificationMessage)
        {
            Console.WriteLine("--> Determining event");

            var eventType = JsonSerializer.Deserialize<GenericEventDto>(notificationMessage);

            switch (eventType.Event)
            {
                case "Platform_Published":
                    Console.WriteLine("--> Platform published event detected");
                    return EventType.PlatformPublished;
                default:
                    Console.WriteLine("--> Could not determine event type");
                    return EventType.Undetermined;
            }
        }

        private void AddPlatform(string platformPublishedMessage)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                 var repo = scope.ServiceProvider.GetRequiredService<ICommandRepo>();

                 var platformPublishedDto = JsonSerializer.Deserialize<PlatformPublishedDto>(platformPublishedMessage);

                 try
                 {
                      var plat = mapper.Map<Platform>(platformPublishedDto);

                      if (!repo.ExternalPlatformExists(plat.ExternalID))
                      {
                          repo.CreatePlatform(plat);
                          repo.SaveChanges();

                          Console.WriteLine($"--> Platform added");
                      }
                      else
                      {
                          Console.WriteLine($"--> Platform already exists {plat.ExternalID}");
                      }
                 }
                 catch (Exception ex)
                 {
                     Console.WriteLine($"--> Could not add platform to db {ex.Message}");
                 }
            }
        }
    }

    public enum EventType
    {
        PlatformPublished,
        Undetermined
    }
}