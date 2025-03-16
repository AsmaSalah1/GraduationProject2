using GraduationProject_Core.Dtos.Team;
using GraduationProject_Core.Interfaces;
using GraduationProject_Core.Models;
using GraduationProject_Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraduationProject_Infrastructure.Repositories
{
	public class TeamRepositry : ITeamRepository
	{
		private readonly ApplicationDbContext dbContext;

		public TeamRepositry(ApplicationDbContext dbContext)
		{
			this.dbContext = dbContext;
		}
		public async Task<bool> AddTeamAsync(CreateTeamDto dto, int userId)
		{
			var team = new Team
			{
				TeamName = dto.TeamName,
				UniversityName = dto.UniversityName,
				Coach = dto.Coach,
				TeamsParticipant = new List<TeamParticipant>() // حل مشكلة NullReferenceException
			};

			if (dto.Participants.Count < 1 || dto.Participants.Count > 3 || dto.Participants==null)
				return false; // عدد المشاركين غير صحيح

			foreach (var participantDto in dto.Participants)
			{
				//var participant = await dbContext.Participants.FindAsync(participantDto.ParticipantId);
				//if (participant == null)
				
					var participant = new Participant
					{
						// ParticipantId = participantDto.ParticipantId,
						ParticipantName = participantDto.ParticipantName
					};
					await dbContext.Participants.AddAsync(participant);
					await dbContext.SaveChangesAsync(); // حفظ المشارك الجديد قبل الإضافة للفريق
				

				team.TeamsParticipant.Add(new TeamParticipant
				{
					TeamId=team.TeamId,
					ParticipantId = participant.ParticipantId,
					Year = dto.Year
				});
			}

			await dbContext.Teams.AddAsync(team);
			await dbContext.SaveChangesAsync();
			return true;
		}


		public async Task<bool> DeleteTeamAsync(int teamId, int userId)
		{
			var team = await dbContext.Teams
							.Include(t => t.TeamsParticipant)
							.FirstOrDefaultAsync(t => t.TeamId == teamId);

			if (team == null)
			{
				return false; // الفريق غير موجود
			}

			dbContext.Teams.Remove(team);
			await dbContext.SaveChangesAsync();
			return true;
		}

		public Task<bool> DeleteTeamAsync(string teamName, int userId)
		{
			throw new NotImplementedException();
		}

		public async Task<IEnumerable<TeamDetailsDto>> GetAllTeamsAsync()
		{
			var teams = await dbContext.Teams
				.Include(t => t.TeamsParticipant)
				.ThenInclude(tp => tp.Participant)
				.ToListAsync();

			return teams.Select(team => new TeamDetailsDto
			{
				TeamName = team.TeamName,
				UniversityName = team.UniversityName,
				Coach = team.Coach,
				Participants = team.TeamsParticipant.Select(tp => tp.Participant.ParticipantName).ToList()
			}).ToList();
		}

		public Task<Team> GetTeamByIdAsync(int teamId)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<Team>> GetTeamsByParticipantIdAsync(int participantId)
		{
			throw new NotImplementedException();
		}
	}
}
