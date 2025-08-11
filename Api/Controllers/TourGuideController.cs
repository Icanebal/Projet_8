using GpsUtil.Location;
using Microsoft.AspNetCore.Mvc;
using TourGuide.DTOs;
using TourGuide.LibrairiesWrappers.Interfaces;
using TourGuide.Services.Interfaces;
using TourGuide.Users;
using TripPricer;

namespace TourGuide.Controllers;

[ApiController]
[Route("[controller]")]
public class TourGuideController : ControllerBase
{
    private readonly ITourGuideService _tourGuideService;
    private readonly IRewardsService _rewardsService;
    private readonly IRewardCentral _rewardCentral;

    public TourGuideController(ITourGuideService tourGuideService, IRewardsService rewardsService, IRewardCentral rewardCentral)
    {
        _tourGuideService = tourGuideService;
        _rewardsService = rewardsService;
        _rewardCentral = rewardCentral;
    }

    [HttpGet("Location")]
    public ActionResult<VisitedLocation> GetLocation([FromQuery] string userName)
    {
        var location = _tourGuideService.GetUserLocation(GetUser(userName));
        return Ok(location);
    }

    [HttpGet("NearbyAttractions")]
    public ActionResult<List<Attraction>> GetNearbyAttractions([FromQuery] string userName)
    {
        var user = GetUser(userName);
        var visitedLocation = _tourGuideService.GetUserLocation(user);
        var attractions = _tourGuideService.GetNearByAttractions(visitedLocation);

        var response = attractions.Select(attraction => new NearbyAttractionResponse
        {
            AttractionName = attraction.AttractionName,
            AttractionLatitude = attraction.Latitude,
            AttractionLongitude = attraction.Longitude,
            UserLatitude = visitedLocation.Location.Latitude,
            UserLongitude = visitedLocation.Location.Longitude,
            DistanceInMiles = _rewardsService.GetDistance(visitedLocation.Location, attraction),
            RewardPoints = _rewardCentral.GetAttractionRewardPoints(attraction.AttractionId, visitedLocation.UserId)
        }).ToList();

        return Ok(response);
    }

    [HttpGet("Rewards")]
    public ActionResult<List<UserReward>> GetRewards([FromQuery] string userName)
    {
        var rewards = _tourGuideService.GetUserRewards(GetUser(userName));
        return Ok(rewards);
    }

    [HttpGet("TripDeals")]
    public ActionResult<List<Provider>> GetTripDeals([FromQuery] string userName)
    {
        var deals = _tourGuideService.GetTripDeals(GetUser(userName));
        return Ok(deals);
    }

    private User GetUser(string userName)
    {
        return _tourGuideService.GetUser(userName);
    }
}
