var CampaignApp = angular.module("CampaignApp", ['ngResource']);

CampaignApp.controller('Campaign', function ($scope, $resource) {
    console.log($resource);
    Campaign = $resource('/JSON/Campaign/:id', { userId: '@id' });
    $scope.Campaign = Campaign.get({ id: window.CampaignID }, function () {
    });
});