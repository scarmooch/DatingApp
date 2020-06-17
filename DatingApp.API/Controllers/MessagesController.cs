using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Route("api/users/{userId}/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        public MessagesController(IDatingRepository repository, IMapper mapper)
        {
            this._mapper = mapper;
            this._repo = repository;
        }

        [HttpGet("{id}", Name = "GetMessage")]
        public async Task<IActionResult> GetMessage(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messageFromRepo = await _repo.GetMessage(id);
            if(messageFromRepo == null)
                return NotFound();
            return Ok(messageFromRepo);
        }

        [HttpGet]
        public async Task<IActionResult> GetMessagesForUser(int userId, [FromQuery] MessageParams messageParams)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            messageParams.UserId = userId;
            var messagesFromRepo = await _repo.GetMessagesForUser(messageParams);
            var messages = _mapper.Map<IEnumerable<MessageForReturnDto>>(messagesFromRepo);

            Response.AddPagination(messagesFromRepo.CurrentPage, 
                                    messagesFromRepo.PageSize, 
                                    messagesFromRepo.TotalCount, 
                                    messagesFromRepo.TotalPages);

            return Ok(messages);
           
        }

        // IMPORTANT: must add another path to differenciate from GetMessage(), as both GET take a single param (id)
        // IMPORTANT: must use userId, not userId1, as this is automatically set to the user logged in
        [HttpGet("thread/{userId2}")]
        public async Task<IActionResult> GetMessageThread(int userId, int userId2)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messagesFromRepo = await _repo.GetMessageThread(userId, userId2);

            var messageThread = _mapper.Map<IEnumerable<MessageForReturnDto>>(messagesFromRepo);
            return Ok(messageThread);
        }


        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userId, /*[FromForm]*/ MessageForCreationDto messageForCreationDto)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            // getting user (who is actually the message sender) from Repo ensures that
            // the autoMapper correctly interprets the senderKnownAs and senderPhotoUrl for the messageToReturn
            // even though userFromRepo is not used in method
            var userFromRepo = await _repo.GetUser(userId);

            messageForCreationDto.SenderId = userId;

            var recipientFromRepo = await _repo.GetUser(messageForCreationDto.RecipientId);
            if(recipientFromRepo == null)
                return BadRequest("Could not find recipient");


            var message = _mapper.Map<Message>(messageForCreationDto);

            _repo.Add(message);

            if (await _repo.SaveAll())
            {
                var messageToReturn = _mapper.Map<MessageForReturnDto>(message);
                return CreatedAtRoute("GetMessage", new { userId = userId, id = message.Id }, messageToReturn);
            }
            throw new System.Exception("Createing the message failed on save");
            //return BadRequest("Could not add the message");
            
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> DeleteMessage(int id, int userId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messageFromRepo = await _repo.GetMessage(id);
            if(messageFromRepo.SenderId == userId)
                messageFromRepo.SenderDeleted = true;
            if(messageFromRepo.RecipientId == userId)
                messageFromRepo.RecipientDeleted = true;
            if(messageFromRepo.SenderDeleted && messageFromRepo.RecipientDeleted)
                _repo.Delete(messageFromRepo);
            
            if(await _repo.SaveAll())
                return NoContent();

            throw new System.Exception("Error deleting the message");
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkMessageAsRead(int id, int userId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            var messageFromRepo = await _repo.GetMessage(id);
            // only recipient can mark as read
            if (messageFromRepo.RecipientId != userId)
                return Unauthorized();
            messageFromRepo.IsRead = true;
            messageFromRepo.DateRead = DateTime.Now;

            await _repo.SaveAll();

            return NoContent();
        }
    }
}