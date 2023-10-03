import React, { useEffect, useState , Component } from 'react';

import "react-image-gallery/styles/css/image-gallery.css";
import ImageGallery from 'react-image-gallery';
import Config from '../../../helpers/Config';

//ProductDetailImages

// const images = [
//     {
//         original: 'https://picsum.photos/id/1018/1000/600/',
//         thumbnail: 'https://picsum.photos/id/1018/250/150/',
//     },
//     {
//         original: 'https://picsum.photos/id/1015/1000/600/',
//         thumbnail: 'https://picsum.photos/id/1015/250/150/',
//     },
//     {
//         original: 'https://picsum.photos/id/1019/1000/600/',
//         thumbnail: 'https://picsum.photos/id/1019/250/150/',
//     },
// ];



const ProductDetailImages = (props) => {
    const [imagesList, setImagesList] = useState([]);
    const [adminPanelBaseURL, setadminPanelBaseURL] = useState(Config['ADMIN_BASE_URL']);

   
    const MakeImageList = () => {
        
        let arrayData = [];
        if(props.ProductImages!=undefined && props.ProductImages!=null && props.ProductImages.length>0){
            
            for (let i = 0; i < props.ProductImages.length; i++) {
                
                arrayData.push({
                    original: (adminPanelBaseURL+ props.ProductImages[i].AttachmentURL),
                    thumbnail: (adminPanelBaseURL+ props.ProductImages[i].AttachmentURL)
                });
            }
        }
       
        setImagesList(arrayData);
       
    }

    useEffect(() => {
        MakeImageList();
    }, [props.ProductImages])

    return (
        <>
            <div className="col-lg-6 col-md-6">
                {
                    imagesList?.length > 0 ?
                        <ImageGallery items={imagesList}  />
                        :
                        <>
                        </>
                }


            </div>
        </>
    );
}
export default ProductDetailImages;