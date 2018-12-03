var page = 0,
    inCallback = false,
    hasReachedEndOfInfiniteScroll = false;

function loadMoreToInfiniteScrollTable(loadMoreRowsUrl, filter, checkfilter) {
    if (page > -1 && !inCallback) {
        inCallback = true;
        page++;
        $("div#loading").show();
        $.ajax({
            type: 'GET',
            url: loadMoreRowsUrl,
            data: "filter=" + filter + "&page=" + page + "&checkfilter=" + checkfilter, 
            success: function (data, textstatus) {
                if (data != '') {
                    $(".relatedcampaign").append(data);
                }
                else {
                    page = -1;
                    showNoMoreRecords = false;
                    $(".load-more-campaign").hide();
                }

                inCallback = false;
                //$("div#loading").hide();
            },
            error: function (XMLHttpRequest, textStatus, errorThrown) {
            }
        });
    }
}

function showNoMoreRecords() {
    hasReachedEndOfInfiniteScroll = true;
}