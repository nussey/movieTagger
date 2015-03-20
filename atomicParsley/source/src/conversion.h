//==================================================================//
/*
   AtomicParsley - conversion.h

   AtomicParsley is GPL software; you can freely distribute,
   redistribute, modify & use under the terms of the GNU General
   Public License; either version 2 or its successor.

   AtomicParsley is distributed under the GPL "AS IS", without
   any warranty; without the implied warranty of merchantability
   or fitness for either an expressed or implied particular purpose.

   Please see the included GNU General Public License (GPL) for
   your rights and further details; see the file COPYING. If you
   cannot, write to the Free Software Foundation, 59 Temple Place
   Suite 330, Boston, MA 02111-1307, USA.  Or www.fsf.org

   Copyright ©2012
   with contributions from others; see the CREDITS file
 */
//==================================================================//

#ifndef CONVERSION_H_
#define CONVERSION_H_

#define ENDPTR 0
#define BASE 10

inline
unsigned long long
strtoullMax(const char *nptr,
        char **endptr,
        int base,
        unsigned long long maxval) {
    unsigned long long ret = strtol(nptr, endptr, base);
        if (ret > maxval) {
            ret = maxval;
                // errrno = ERANGE;
        } else {
            if (ret == ULONG_MAX && errno == ERANGE)
                ret = maxval;
        }
    return ret;
}

#define uint64_from_string(NPTR) \
strtoullMax(NPTR, ENDPTR, BASE, (uint64_t)-1)

#define uint32_from_string(NPTR) \
strtoullMax(NPTR, ENDPTR, BASE, (uint32_t)-1)

#define uint16_from_string(NPTR) \
strtoullMax(NPTR, ENDPTR, BASE, (uint16_t)-1)

#define uint8_from_string(NPTR) \
strtoullMax(NPTR, ENDPTR, BASE, (uint8_t)-1)

#endif /* CONVERSION_H_ */
