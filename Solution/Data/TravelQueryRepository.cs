﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

using WebApiQueryMongoDb.Interfaces;
using WebApiQueryMongoDb.Model;
using MongoDB.Bson;
using System.Linq;

namespace WebApiQueryMongoDb.Data
{
    public class TravelQueryRepository : ITravelQueryRepository
    {
        private readonly TravelQueryContext _context = null;

        public TravelQueryRepository(IOptions<Settings> settings)
        {
            _context = new TravelQueryContext(settings);
        }

        public async Task<IEnumerable<TravelItem>> GetTravelItems()
        {
            try
            {
                return await _context.TravelItemsLinq.Take(500).ToListAsync();
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        public async Task<IEnumerable<TravelItem>> GetTravelItems(string cityName, string action)
        {
            try
            {
                if (action != null)
                    return await _context.TravelItemsLinq
                        .Where(p => p.City == cityName && p.Action == action).ToListAsync();

                return await _context.TravelItemsLinq.Where(p => p.City == cityName)
                    .OrderBy(p => p.Action)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        public async Task<IEnumerable<object>> GroupTravelItems(string cityName)
        {
            try
            {
                return await _context.TravelItemsLinq
                                    .Where(p => p.City == cityName)
                                    .GroupBy(grp => new { grp.City, grp.Action })
                                    .Select(g => new { g.Key.City, g.Key.Action }).ToListAsync();
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        public async Task<IEnumerable<object>> GetCitiesInitial(string countryCode, int minPopulation = 0)
        {
            try
            {
                var filterBuilder = Builders<City>.Filter;
                var filter = filterBuilder.Eq(x => x.CountryCode, countryCode)
                                & filterBuilder.Gte(x => x.Population, minPopulation)
                                & filterBuilder.Gte(x => x.Id, ObjectId.Parse("58fc8ae631a8a6f8d000f9c3"));

                var query = _context.Cities.Find(filter)
                                            .SortByDescending(p => p.Id)
                                            .Limit(200);
                var items = await query.ToListAsync();

                return items.Select(x => new
                {
                    BsonId = x.Id.ToString(),
                    Timestamp = x.Id.Timestamp,
                    ServerUpdatedOn = x.Id.CreationTime,
                    x.Name,
                    x.CountryCode,
                    x.Population
                });
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        public async Task<IEnumerable<object>> GetCities(string countryCode, string lastBsonId, int minPopulation = 0)
        {
            try
            {
                var filterBuilder = Builders<City>.Filter;
                var filter = filterBuilder.Eq(x => x.CountryCode, countryCode)
                                & filterBuilder.Gte(x => x.Population, minPopulation)
                                & filterBuilder.Lte(x => x.Id, ObjectId.Parse(lastBsonId));

                var query = _context.Cities.Find(filter)
                                            .SortByDescending(p => p.Id)
                                            .Limit(200);

                var items = await query.ToListAsync();

                return items.Select(x => new
                {
                    BsonId = x.Id.ToString(),
                    Timestamp = x.Id.Timestamp,
                    ServerUpdatedOn = x.Id.CreationTime,
                    x.Name,
                    x.CountryCode,
                    x.Population
                });
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        public async Task<IEnumerable<object>> GetCitiesInitialLinq(string countryCode, int minPopulation = 0)
        {
            try
            {
                var query = _context.CitiesLinq
                                    .Where(x => x.CountryCode == countryCode
                                                && x.Population >= minPopulation)
                                    .OrderByDescending(x => x.Id)
                                    .Take(200);

                var items = await query.ToListAsync();
                return items.Select(x => new
                {
                    BsonId = x.Id.ToString(),
                    Timestamp = x.Id.Timestamp,
                    ServerUpdatedOn = x.Id.CreationTime,
                    x.Name,
                    x.CountryCode,
                    x.Population
                });
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }

        public async Task<IEnumerable<object>> GetCitiesLinq(string countryCode, string lastBsonId, int minPopulation = 0)
        {
            try
            {
                var query = _context.CitiesLinq
                                    .Where(x => x.CountryCode == countryCode
                                             && x.Population >= minPopulation
                                             && x.Id <= ObjectId.Parse(lastBsonId))
                                    .OrderByDescending(x => x.Id)
                                    .Take(200);
                var items = await query.ToListAsync();

                return items.Select(x => new
                                    {
                                        BsonId = x.Id.ToString(),
                                        Timestamp = x.Id.Timestamp,
                                        ServerUpdatedOn = x.Id.CreationTime,
                                        x.Name,
                                        x.CountryCode,
                                        x.Population
                                    });
            }
            catch (Exception ex)
            {
                // log or manage the exception
                throw ex;
            }
        }
    }
}