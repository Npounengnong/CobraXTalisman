import React, { useEffect, useState } from 'react';
import { Helmet } from 'react-helmet';
import Config from '../../../helpers/Config';
import SiteBreadcrumb from '../../components/layout/SiteBreadcrumb';
import BestFacilities from '../../components/shared/BestFacilities';
import about1 from '../../resources/themeContent/images/theme-images/about1.jpg';
import about2 from '../../resources/themeContent/images/theme-images/about2.jpg';
import signature from '../../resources/themeContent/images/theme-images/signature.png';


const About = () => {
  const [siteTitle, setSiteTitle] = useState(Config['SITE_TTILE']);

  return (
    <>


      <Helmet>
        <title>{siteTitle} - About Us</title>
        <meta name="description" content={siteTitle + " - About us page"}  />
        <meta name="keywords" content="About us"></meta>
      </Helmet>

      <SiteBreadcrumb title="About Us" />

      <section className="about-area ptb-60">
        <div className="container">
          <div className="row align-items-center">
            <div className="col-lg-6 col-md-12">
              <div className="about-content">
                <h2>About Our Store</h2>
                <p>Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book.</p>

                <p>It has survived not only five centuries, but also the leap into electronic typesetting, remaining essentially unchanged.</p>

                <p>Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since the 1500s, when an unknown printer took a galley of type and scrambled it to make a type specimen book.</p>

                <div className="signature mb-0">
                  <img src={signature} alt="image" />
                </div>
              </div>
            </div>

            <div className="col-lg-6 col-md-12">
              <div className="about-image">
                <img src={about1} className="about-img1" alt="image" />
                <img src={about2} className="about-img2" alt="image" />
              </div>
            </div>
          </div>
        </div>
      </section>



      <BestFacilities />

    </>
  );

}

export default About;
