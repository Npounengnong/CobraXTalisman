import React, { useEffect, useState } from 'react';
import { Helmet } from 'react-helmet';
import { useDispatch } from 'react-redux';
import { useParams } from 'react-router-dom';
import { MakeApiCallSynchronous , MakeApiCallAsync } from '../../../helpers/ApiHelpers';
    
import Config from '../../../helpers/Config';
import { LOADER_DURATION } from '../../../helpers/Constants';
import rootAction from '../../../stateManagment/actions/rootAction';
import SiteBreadcrumb from '../../components/layout/SiteBreadcrumb';
import BestFacilities from '../../components/shared/BestFacilities';



const Campaign = () => {
    const dispatch = useDispatch();
    const [siteTitle, setSiteTitle] = useState(Config['SITE_TTILE']);
    const [CampaignDetail, setCampaignDetail] = useState({});
    const [adminPanelBaseURL, setBaseUrl] = useState(Config['ADMIN_BASE_URL']);

    //--set product id from url
    const params = useParams();
    const [CampaignId, setCampaignId] = useState(params.campaign_id ?? 0);

    useEffect(() => {
        // declare the data fetching function
        const getCampaignDetail = async () => {

             
            const headers = {
                Accept: 'application/json',
                'Content-Type': 'application/json',
                   
            }


            const param = {
                requestParameters: {
                    CampaignId: CampaignId,
                    recordValueJson: "[]",
                },
            };



            //--Get product detail
            const response = await MakeApiCallAsync(Config.END_POINT_NAMES['GET_WEB_CAMPAIGN_DETAIL'], null, param, headers, "POST", true);
            if (response != null && response.data != null) {
                let detail = JSON.parse(response.data.data);
                await setCampaignDetail(detail[0]);


            }

        }

        //--start loader
        dispatch(rootAction.commonAction.setLoading(true));

        // call the function
        getCampaignDetail().catch(console.error);

        //--stop loader
        setTimeout(() => {
            dispatch(rootAction.commonAction.setLoading(false));
        }, LOADER_DURATION);


    }, [])


    return (
        <>


            <Helmet>
                <title>{siteTitle} - Campaign</title>
                <meta name="description" content={siteTitle + " - Campaign"} />
                <meta name="keywords" content="Campaign"></meta>
            </Helmet>

            <SiteBreadcrumb title="Campaign" />

            <section className="about-area ptb-60">
                <div className="container">
                    <div className="row align-items-center">
                        <div className="col-lg-6 col-md-12">
                            <div className="about-content">
                                <h2>{CampaignDetail.MainTitle}</h2>
                                <h3>{CampaignDetail.DiscountTitle}</h3>
                                <p>{CampaignDetail.Body}</p>
                                <div className="signature mb-0">

                                </div>
                            </div>
                        </div>

                        <div className="col-lg-6 col-md-12">
                            <div className="about-image">
                                <img src={adminPanelBaseURL + CampaignDetail.CoverPictureUrl} className="about-img1" alt="image" />
                              
                            </div>
                        </div>
                    </div>
                </div>
            </section>



            <BestFacilities />

        </>
    );

}

export default Campaign;
