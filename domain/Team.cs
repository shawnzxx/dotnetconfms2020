using SharedKernel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Domain
{
    public class Team
    {
        //Private parameterless constructor
        //No body can instantiate team without provide parameters list below
        private Team()
        {
        }

        //Business rules create team must have below values
        public Team(string teamName, string nickname, string yearFounded, string homeStadium)
        {
            _teamname = teamName;
            Nickname = nickname;
            YearFounded = yearFounded;
            HomeStadium = homeStadium;
            _id = Guid.NewGuid();
            _players = new List<Player>();
        }

        private Guid _id;
        //no body needs to be reading the Id from outside when they are looking at Team object
        //Since it is private property ef won't find it's related property Id
        //so we need to set backing filed mapping in configuration
        private Guid Id => _id;

        //different between this and above Id, I want outside to read TeamName
        //If it have getter ef core will figure it out map to which backing filed
        //but without getter we need to provide the filed name use HasField
        private string _teamname;
        public string TeamName => _teamname;

        public string Nickname { get; private set; }
        public string YearFounded { get; private set; }
        public string HomeStadium { get; private set; }

        // EF Core recognizes IEnumerable, but as of EFC3,  backing field is default for read/write anyway
        //Players property is a "defensive copy", users can't modify the field
        public IEnumerable<Player> Players => _players.ToList();

        private ICollection<Player> _players;  // we can manipulate the ICollection locally

        public bool AddPlayer(string firstName, string lastname, out string response)
        {
            if (_players == null)
            {
                //this can only be tested with integration test against EF Core
                response = "You must first retrieve this team's existing list of players";
                return false;
            }
            var fullName = PersonFullName.Create(firstName, lastname).FullName;
            var foundPlayer = _players.Where(p => p.Name.Equals(fullName)).FirstOrDefault();
            if (foundPlayer == null)
            {
                _players.Add(new Player(firstName, lastname));
                response = "Player added to team";
                return true;
            }
            else
            {
                response = "Duplicate player";
                return false;
            }
        }

        //very protective, we don't even have Manager property, only have filed
        //outside access only manage's name
        //if they want to chang manager need use method
        private Manager _manager;
        public string ManagerName => _manager.Name;

        public void ChangeManagement(Manager newManager)
        {
            if (_manager is null || _manager.Name != newManager.Name)
            {
                _manager?.RemoveFromTeam(Id);
                newManager.BecameTeamManager(Id);
                _manager = newManager;
            }
        }

        public UniformColors HomeColors { get; private set; }

        public void SpecifyHomeUniformColors(Color primary, Color secondary)
        {
            //would be interesting in another aggregate in this same bounded context
            //(but not necessarily the same microservice)
            //to validate a rule ensuring no two teams have the same color pair
            HomeColors = new UniformColors(primary, secondary);
        }
    }
}