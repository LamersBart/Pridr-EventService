using System.IdentityModel.Tokens.Jwt;
using AutoMapper;
using EventService.Data;
using EventService.DTOs;
using EventService.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace EventService.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EventController : ControllerBase
{
    private readonly IUserEventRepo _repo;
    private readonly IMapper _mapper;

    public EventController(IUserEventRepo repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    [HttpGet("getAll/")]
    public ActionResult<IEnumerable<UserEventReadDto>> GetEvents() 
    {
        Console.WriteLine("--> Getting all Events...");
        IEnumerable<UserEvent> events = _repo.getAllUserEvents();
        return Ok(_mapper.Map<IEnumerable<UserEventReadDto>>(events));
    }
    
    [HttpGet("get/{eventId}")]
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
    
    [HttpPost("create")]
    public async Task<ActionResult<UserEventReadDto>> CreateEvent(UserEventCreateDto userEventCreateDto)
    {
        string? accessToken = await HttpContext.GetTokenAsync("access_token");
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
    
    [HttpPatch("update/{eventId}")]
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
            foreach (int profileId in userEventAddProfileDto.ProfileIds)
            {
                if (!userEvent.ProfileIds!.Contains(profileId))
                {
                    userEvent.ProfileIds!.Add(profileId);
                }
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
            foreach (int profileId in userEventAddProfileDto.ProfileIds)
            {
                if (userEvent.ProfileIds!.Contains(profileId))
                {
                    userEvent.ProfileIds.Remove(profileId);
                }
            }
        }
        _repo.UpdateUserEvent(userEvent);
        _repo.SaveChanges();
        return Ok(_mapper.Map<UserEventReadDto>(userEvent));
    }
    
    [HttpDelete("delete/{eventId}")]
    public ActionResult DeleteEvent(int eventId)
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

    private Dictionary<string, object> DecodeJwt(string bearerToken)
    {
        try
        {
            var token = bearerToken.StartsWith("Bearer ") ? bearerToken.Substring(7) : bearerToken;
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var claims = jwtToken.Claims.ToDictionary(c => c.Type, c => (object)c.Value);
            claims["iss"] = jwtToken.Issuer;
            claims["aud"] = string.Join(", ", jwtToken.Audiences);
            return claims;
        }
        catch (Exception ex)
        {
            throw new ArgumentException("Invalid token", ex);
        }
    }
    private string FetchKeycloakUserId(string bearerToken)
    {
        Dictionary<string, object> dictionary = DecodeJwt(bearerToken);
        if (dictionary.TryGetValue("sub", out object? subValue))
        {
            return subValue.ToString() ?? throw new InvalidOperationException();
        }
        throw new InvalidOperationException();
    }
}