import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
// import Link from 'next/link';
import { useSelector, useDispatch } from 'react-redux';
import { makeAnyStringLengthShort, makeProductShortDescription } from '../../../helpers/ConversionHelper';
import Config from '../../../helpers/Config';
import { MakeApiCallSynchronous , MakeApiCallAsync } from '../../../helpers/ApiHelpers';
import rootAction from '../../../stateManagment/actions/rootAction';
import { LOADER_DURATION } from '../../../helpers/Constants';
    

const CompaignSection = () => {
    const dispatch = useDispatch();
    const [CampaignList, setCampaignList] = useState([]);
    const [adminPanelBaseURL, setBaseUrl] = useState(Config['ADMIN_BASE_URL']);

    useEffect(() => {
        // declare the data fetching function
        const getCampaignList = async () => {

             
            const headersCampaign = {
                // customerid: userData?.UserID,
                // customeremail: userData.EmailAddress,
                Accept: 'application/json',
                'Content-Type': 'application/json',
                   
            }


            const paramCampaign = {
                requestParameters: {
                    recordValueJson: "[]",
                },
            };

            const response = await MakeApiCallAsync(Config.END_POINT_NAMES['GET_WEB_CAMPAIGN_LIST'], null, paramCampaign, headersCampaign, "POST", true);
            if (response != null && response.data != null) {
                await setCampaignList(JSON.parse(response.data.data));
                console.log(JSON.parse(response.data.data));
            }


        }

        //--start loader
        dispatch(rootAction.commonAction.setLoading(true));

        // call the function
        getCampaignList().catch(console.error);

        //--stop loader
        setTimeout(() => {
            dispatch(rootAction.commonAction.setLoading(false));
        }, LOADER_DURATION);

    }, [])


    return (
        <>
            <div className="categories-banner-area pt-60 pb-30 bg-fcfbfb">
                <div className="container-fluid">



                    {
                        CampaignList != undefined && CampaignList.length > 0
                            ?
                            <div className="row">

                                {
                                    CampaignList?.map((item, idx) =>

                                        <div className="col-lg-3 col-sm-6">
                                            <div className="offer-categories-box left-categories hover-active">
                                                <img src={adminPanelBaseURL + item.CoverPictureUrl} alt="image" height={190}/>
                                                <div className="content">
                                                    <span>{ makeAnyStringLengthShort(item.DiscountTitle, 30)}</span>
                                                    <h3>{item.MainTitle}</h3>
                                                    <Link to={`/campaign/${item.CampaignId}/${item.MainTitle}`} className="btn btn-primary">View Detail</Link>
                                                </div>

                                                <Link to={`/campaign/${item.CampaignId}/${item.MainTitle}`}  className="link-btn">

                                                </Link>
                                            </div>
                                        </div>

                                    )}

                            </div>
                            :
                            <>
                            </>
                    }



                  

                </div>
            </div>
        </>
    );

}


export default CompaignSection;
