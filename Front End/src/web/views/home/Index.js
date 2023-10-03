import React, { useEffect, useState } from 'react';
import BannerSlider from '../../components/home/BannerSlider'
import PopularProducts from '../../components/products/PopularProducts';
import CompaignSection from '../../components/shared/CompaignSection';
import NewProducts from '../../components/products/NewProducts';
import PopularCategories from '../../components/shared/PopularCategories';
import BestFacilities from '../../components/shared/BestFacilities';
import SubscribeNewsLetter from '../../components/shared/SubscribeNewsLetter';
import { Link, useNavigate } from 'react-router-dom';
import { Helmet } from 'react-helmet';
import Config from '../../../helpers/Config';


const Home = () => {
    const navigate = useNavigate();
    const [siteTitle, setSiteTitle] = useState(Config['SITE_TTILE']);


    const handleBannerButtonClickUrl = (url) => {
            
        if (url != undefined && url != null && url.length > 0) {
            window.location.href = url;
        } else {
            return false;
        }

    }

    return (
        <>

            <Helmet>
                <title>{siteTitle} - Home</title>
                <meta name="description" content={siteTitle + " - Home"} />
                <meta name="keywords" content="Home"></meta>
            </Helmet>

            <BannerSlider handleBannerButtonClickUrl={handleBannerButtonClickUrl} />

            <PopularCategories />

            <NewProducts />

            <CompaignSection />

            <PopularProducts />

            <BestFacilities />
            <SubscribeNewsLetter />

        </>
    );

}

export default Home;


