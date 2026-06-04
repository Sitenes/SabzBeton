/**
* PHP Email Form Validation - v3.10
* URL: https://bootstrapmade.com/php-email-form/
* Author: BootstrapMade.com
*/
(function () {
  "use strict";

  let forms = document.querySelectorAll('.php-email-form');

  forms.forEach( function(e) {
    e.addEventListener('submit', function(event) {
      event.preventDefault();

      let thisForm = this;

      let action = thisForm.getAttribute('action');
      let recaptcha = thisForm.getAttribute('data-recaptcha-site-key');
      
      if( ! action ) {
        displayError(thisForm, 'The form action property is not set!');
        return;
      }
      thisForm.querySelector('.loading').classList.add('d-block');
      thisForm.querySelector('.error-message').classList.remove('d-block');
      thisForm.querySelector('.sent-message').classList.remove('d-block');

      let formData = new FormData( thisForm );

      if ( recaptcha ) {
        if(typeof grecaptcha !== "undefined" ) {
          grecaptcha.ready(function() {
            try {
              grecaptcha.execute(recaptcha, {action: 'php_email_form_submit'})
              .then(token => {
                formData.set('recaptcha-response', token);
                php_email_form_submit(thisForm, action, formData);
              })
            } catch(error) {
              displayError(thisForm, error);
            }
          });
        } else {
          displayError(thisForm, 'The reCaptcha javascript API url is not loaded!')
        }
      } else {
        php_email_form_submit(thisForm, action, formData);
      }
    });
  });

  function php_email_form_submit(thisForm, action, formData) {
    fetch(action, {
      method: 'POST',
      body: formData,
      headers: {'X-Requested-With': 'XMLHttpRequest'}
    })
    .then(async response => {
      const rawText = await response.text();
      let parsedResponse = null;

      try {
        parsedResponse = rawText ? JSON.parse(rawText) : null;
      } catch (error) {
        parsedResponse = null;
      }

      if (response.ok) {
        if (parsedResponse && parsedResponse.success === true) {
          return parsedResponse;
        }

        if (rawText.trim() === 'OK') {
          return { success: true, message: 'Your message has been sent.' };
        }

        throw new Error(parsedResponse?.message || rawText || `Form submission failed from: ${action}`);
      }

      throw new Error(parsedResponse?.message || rawText || `${response.status} ${response.statusText} ${response.url}`);
    })
    .then(data => {
      thisForm.querySelector('.loading').classList.remove('d-block');
      const successMessage = thisForm.querySelector('.sent-message');
      const errorMessage = thisForm.querySelector('.error-message');

      if (successMessage) {
        successMessage.textContent = data?.message || successMessage.textContent;
        successMessage.classList.add('d-block');
      }

      if (errorMessage) {
        errorMessage.classList.remove('d-block');
        errorMessage.textContent = '';
      }

      thisForm.reset();

      if (typeof grecaptcha !== 'undefined' && typeof grecaptcha.reset === 'function') {
        grecaptcha.reset();
      }
    })
    .catch((error) => {
      displayError(thisForm, error);
    });
  }

  function displayError(thisForm, error) {
    thisForm.querySelector('.loading').classList.remove('d-block');
    thisForm.querySelector('.sent-message').classList.remove('d-block');

    const errorBox = thisForm.querySelector('.error-message');
    if (errorBox) {
      const message = error && error.message ? error.message : String(error);
      errorBox.textContent = message;
      errorBox.classList.add('d-block');
    }
  }

})();
