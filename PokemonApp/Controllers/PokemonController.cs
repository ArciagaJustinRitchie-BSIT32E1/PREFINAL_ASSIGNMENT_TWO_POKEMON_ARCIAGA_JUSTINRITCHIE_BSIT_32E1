using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using PokemonApp.Models;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace PokemonApp.Controllers
{
    public class PokemonController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public PokemonController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("PokeApiClient");
                var response = await client.GetStringAsync($"https://pokeapi.co/api/v2/pokemon?offset={(page - 1) * 20}&limit=20");

                var data = JObject.Parse(response);
                var pokemonList = data["results"].Select(p => new Pokemon
                {
                    Name = p["name"].ToString(),
                    Moves = new List<string>(),  // Moves and Abilities can be fetched on the details page
                    Abilities = new List<string>()
                }).ToList();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling((double)(data["count"].Value<int>() / 20));

                return View(pokemonList);
            }
            catch (Exception ex)
            {
                // Log the exception and return an error view
                // You can log the error using any logging framework (e.g., Serilog, NLog)
                // For simplicity, we'll just return an error message to the view

                ViewBag.ErrorMessage = "An error occurred while fetching the Pokemon data. Please try again later.";
                return View("Error");
            }
        }

        public async Task<IActionResult> Details(string name)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("PokeApiClient");
                var response = await client.GetStringAsync($"https://pokeapi.co/api/v2/pokemon/{name}");

                var data = JObject.Parse(response);
                var pokemon = new Pokemon
                {
                    Name = data["name"].ToString(),
                    Moves = data["moves"].Select(m => m["move"]["name"].ToString()).ToList(),
                    Abilities = data["abilities"].Select(a => a["ability"]["name"].ToString()).ToList()
                };

                return View(pokemon);
            }
            catch (Exception ex)
            {
                // Log the exception and return an error view
                // For simplicity, we'll just return an error message to the view

                ViewBag.ErrorMessage = "An error occurred while fetching the Pokemon details. Please try again later.";
                return View("Error");
            }
        }
    }
}
