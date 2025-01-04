using System.IdentityModel.Tokens.Jwt;
using AutoMapper;
using EventService.Data;
using EventService.DTOs;
using EventService.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventService.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/event")]
public class EventController : ControllerBase
{
    private readonly IUserEventRepo _repo;
    private readonly IMapper _mapper;

    public EventController(IUserEventRepo repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    [HttpGet()]
    public ActionResult<IEnumerable<UserEventReadDto>> GetEvents() 
    {
        Console.WriteLine("--> Getting all Events...");
        IEnumerable<UserEvent> events = _repo.getAllUserEvents();
        return Ok(_mapper.Map<IEnumerable<UserEventReadDto>>(events));
    }
    
    [HttpGet("{eventId}")]
    public ActionResult<UserEventReadDto> GetEventById(int eventId) 
    {
        if (!_repo.UserEventExist(eventId))
        {
            return NotFound();
        }
        Console.WriteLine($"--> Getting Event By Id: {eventId}...");
        UserEvent userEvent = _repo.GetUserEventById(eventId);
        return Ok(_mapper.Map<UserEventReadDto>(userEvent));
    }
    
    [HttpPost()]
    public async Task<ActionResult<UserEventReadDto>> CreateEvent(UserEventCreateDto userEventCreateDto)
    {
        string? accessToken = await HttpContext.GetTokenAsync("access_token");
        // Converteer de datum naar UTC
        userEventCreateDto.Date = DateTime.SpecifyKind(userEventCreateDto.Date, DateTimeKind.Utc);
        Console.WriteLine($"--> Adding New Event...");
        UserEvent newUserEvent = _mapper.Map<UserEvent>(userEventCreateDto);
        if (accessToken != null)
        {
            newUserEvent.CreatedBy = FetchKeycloakUserId(accessToken);
        }
        _repo.CreateUserEvent(newUserEvent);
        _repo.SaveChanges();
        return Ok(_mapper.Map<UserEventReadDto>(newUserEvent));
    }
    
    [HttpPatch("{eventId}")]
    public ActionResult<UserEventReadDto> UpdateEvent(int eventId, UserEventUpdateDto userEventUpdateDto)
    {
        if (!_repo.UserEventExist(eventId))
        {
            return NotFound();
        }
        Console.WriteLine($"--> Updating Event By Id: {eventId}...");
        UserEvent userEvent = _repo.GetUserEventById(eventId);
        UserEvent updatedUserEvent = _mapper.Map(userEventUpdateDto, userEvent);
        _repo.UpdateUserEvent(updatedUserEvent);
        _repo.SaveChanges();
        return Ok(_mapper.Map<UserEventReadDto>(updatedUserEvent));
    }
    
    [HttpPatch("addProfile/{eventId}")]
    public ActionResult<UserEventReadDto> AddProfileForEvent(int eventId, UserEventAddProfileDto userEventAddProfileDto)
    {
        if (!_repo.UserEventExist(eventId))
        {
            return NotFound();
        }
        Console.WriteLine($"--> Add(ing) Profile(s) For Event By Id: {eventId}...");
        UserEvent userEvent = _repo.GetUserEventById(eventId);
        if (userEventAddProfileDto.ProfileIds != null)
        {
            foreach (string profileId in userEventAddProfileDto.ProfileIds.Where(x => !userEvent.ProfileIds!.Contains(x)))
            {
                userEvent.ProfileIds!.Add(profileId);
            }
        }
        _repo.UpdateUserEvent(userEvent);
        _repo.SaveChanges();
        return Ok(_mapper.Map<UserEventReadDto>(userEvent));
    }
    
    [HttpPatch("removeProfile/{eventId}")]
    public ActionResult<UserEventReadDto> RemoveProfileForEvent(int eventId, UserEventAddProfileDto userEventAddProfileDto)
    {
        if (!_repo.UserEventExist(eventId))
        {
            return NotFound();
        }
        Console.WriteLine($"--> Remove(ing) Profile(s) For Event By Id: {eventId}...");
        UserEvent userEvent = _repo.GetUserEventById(eventId);
        if (userEventAddProfileDto.ProfileIds != null)
        {
            foreach (string profileId in userEventAddProfileDto.ProfileIds.Where(x => userEvent.ProfileIds!.Contains(x)))
            {
                userEvent.ProfileIds!.Remove(profileId);
            }
        }
        _repo.UpdateUserEvent(userEvent);
        _repo.SaveChanges();
        return Ok(_mapper.Map<UserEventReadDto>(userEvent));
    }
    
    [HttpDelete("{eventId}")]
    [ProducesResponseType<int>(StatusCodes.Status200OK)]
    public IActionResult DeleteEvent(int eventId)
    {
        if (!_repo.UserEventExist(eventId))
        {
            return NotFound();
        }
        Console.WriteLine($"--> Deleting Event By Id: {eventId}...");
        _repo.DeleteUserEvent(eventId);
        _repo.SaveChanges();
        return Ok("Event Deleted");
    }

    private static Dictionary<string, object> DecodeJwt(string bearerToken)
    {
        try
        {
            // Strip "Bearer " als prefix
            var token = bearerToken.StartsWith("Bearer ") ? bearerToken.Substring(7) : bearerToken;

            // Lees het token
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);

            // Claims verwerken
            var claims = new Dictionary<string, object>();

            foreach (var claim in jwtToken.Claims)
            {
                // Controleer of de sleutel al bestaat
                if (claims.ContainsKey(claim.Type))
                {
                    // Voeg meerdere waarden toe als lijst
                    if (claims[claim.Type] is List<object> list)
                    {
                        list.Add(claim.Value); // Voeg toe aan bestaande lijst
                    }
                    else
                    {
                        claims[claim.Type] = new List<object> { claims[claim.Type], claim.Value };
                    }
                }
                else
                {
                    // Voeg nieuwe sleutel/waarde toe
                    claims[claim.Type] = claim.Value;
                }
            }

            // Voeg standaardwaarden toe
            claims["iss"] = jwtToken.Issuer;
            claims["aud"] = jwtToken.Audiences.ToList(); // Audiences expliciet als lijst

            return claims;
        }
        catch (Exception ex)
        {
            throw new ArgumentException("Invalid token", ex);
        }
    }
    private static string FetchKeycloakUserId(string bearerToken)
    {
        Dictionary<string, object> dictionary = DecodeJwt(bearerToken);
        if (dictionary.TryGetValue("sub", out object? subValue))
        {
            return subValue.ToString() ?? throw new InvalidOperationException();
        }
        throw new InvalidOperationException();
    }
}